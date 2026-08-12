import type {
  HistoryRecord,
  HistoryTurn,
  ReviseRequest,
  ReviseResult,
} from "../types";

const API_BASE = (import.meta.env.VITE_DDM_BASE_URL || "/ddm").replace(/\/$/, "");
const REQUEST_TIMEOUT_MS = 20_000;

type JsonObject = Record<string, unknown>;

function isObject(value: unknown): value is JsonObject {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function readValue(source: JsonObject, names: string[]): unknown {
  const wanted = new Set(names.map((name) => name.toLowerCase()));
  const entry = Object.entries(source).find(([key]) => wanted.has(key.toLowerCase()));
  return entry?.[1];
}

function readText(source: JsonObject, ...names: string[]): string | undefined {
  const value = readValue(source, names);
  if (typeof value === "string") return value;
  if (typeof value === "number") return String(value);
  return undefined;
}

function readDate(source: JsonObject): Date | undefined {
  const value = readText(
    source,
    "createdAt",
    "createTime",
    "createdTime",
    "createDate",
    "createdDate",
    "insertTime",
    "insertDt",
  );
  if (!value) return undefined;
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? undefined : parsed;
}

async function postJson(path: string, payload: unknown): Promise<{ body: unknown; status: number }> {
  const controller = new AbortController();
  const timeout = window.setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  try {
    const response = await fetch(`${API_BASE}${path}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
      signal: controller.signal,
    });
    const raw = await response.text();
    let body: unknown = raw;

    if (raw) {
      try {
        body = JSON.parse(raw) as unknown;
      } catch {
        body = raw;
      }
    }

    if (!response.ok) {
      const detail = typeof body === "string" ? body : JSON.stringify(body);
      throw new Error(`DDM 回傳 HTTP ${response.status}${detail ? `：${detail}` : ""}`);
    }

    return { body, status: response.status };
  } catch (error) {
    if (error instanceof DOMException && error.name === "AbortError") {
      throw new Error("DDM 回應逾時，請確認服務是否已啟動。");
    }
    if (error instanceof TypeError) {
      throw new Error("無法連線至 DDM，請確認 7079 服務是否已啟動。");
    }
    throw error;
  } finally {
    window.clearTimeout(timeout);
  }
}

function findAnswer(value: unknown): string | null {
  if (typeof value === "string") return value.trim() || null;
  if (Array.isArray(value)) {
    for (const item of value) {
      const result = findAnswer(item);
      if (result) return result;
    }
    return null;
  }
  if (!isObject(value)) return null;

  const preferred = ["revisedText", "answer", "response", "result", "output", "message", "data"];
  for (const name of preferred) {
    const child = readValue(value, [name]);
    if (child !== undefined) {
      const result = findAnswer(child);
      if (result) return result;
    }
  }

  for (const child of Object.values(value)) {
    const result = findAnswer(child);
    if (result) return result;
  }
  return null;
}

function collectHistory(value: unknown, target: HistoryTurnSource[]): void {
  if (Array.isArray(value)) {
    value.forEach((item) => collectHistory(item, target));
    return;
  }
  if (!isObject(value)) return;

  const questionText = readText(value, "questionText", "question", "inputText", "content");
  const answerText = readText(value, "answerText", "answer", "responseText", "revisedText", "response");

  if (questionText || answerText) {
    target.push({
      uniqueId: readText(value, "uniqueId", "historyId", "id", "uuid"),
      threadId: readText(value, "threadId", "conversationThreadId"),
      account: readText(value, "account", "employeeId"),
      chatTitle: readText(value, "chatTitle", "title"),
      questionText: questionText || "",
      answerText: answerText || "",
      originCode: readText(value, "originCode"),
      createdAt: readDate(value),
    });
  }

  Object.values(value).forEach((child) => {
    if (Array.isArray(child) || isObject(child)) collectHistory(child, target);
  });
}

interface HistoryTurnSource extends HistoryTurn {
  threadId?: string;
  account?: string;
  chatTitle?: string;
  originCode?: string;
}

function buildTitle(question: string): string {
  const value = question.trim() || "歷史對話";
  return value.length <= 24 ? value : `${value.slice(0, 24)}…`;
}

function groupHistory(items: HistoryTurnSource[]): HistoryRecord[] {
  const groups = new Map<string, HistoryTurnSource[]>();

  items.forEach((item, index) => {
    const key = item.threadId || item.uniqueId || `legacy-${item.createdAt?.toISOString() || index}`;
    const group = groups.get(key) || [];
    group.push(item);
    groups.set(key, group);
  });

  return Array.from(groups.entries())
    .map(([key, group]) => {
      const turns = [...group].sort(
        (a, b) => (a.createdAt?.getTime() || 0) - (b.createdAt?.getTime() || 0),
      );
      const first = turns[0];
      const latest = turns[turns.length - 1];

      return {
        uniqueId: key,
        threadId: first.threadId || key,
        account: first.account,
        chatTitle: first.chatTitle || buildTitle(first.questionText),
        questionText: latest.questionText,
        answerText: latest.answerText,
        originCode: first.originCode,
        createdAt: latest.createdAt,
        turns: turns.map(({ uniqueId, questionText, answerText, createdAt }) => ({
          uniqueId,
          questionText,
          answerText,
          createdAt,
        })),
      } satisfies HistoryRecord;
    })
    .sort((a, b) => (b.createdAt?.getTime() || 0) - (a.createdAt?.getTime() || 0));
}

export async function reviseText(request: ReviseRequest): Promise<ReviseResult> {
  const { body, status } = await postJson("/ReviseText", request);
  return { answer: findAnswer(body), status };
}

export async function getHistoryByAccount(account: string): Promise<HistoryRecord[]> {
  const { body } = await postJson("/GetHistoryByAccount", {
    account,
    originCode: "DDM",
  });
  const rows: HistoryTurnSource[] = [];
  collectHistory(body, rows);
  return groupHistory(rows);
}
