export type ChatRole = "user" | "assistant";

export interface ChatMessage {
  id: string;
  role: ChatRole;
  text: string;
  createdAt: Date;
  tone?: "default" | "error" | "success";
}

export interface HistoryTurn {
  uniqueId?: string;
  questionText: string;
  answerText: string;
  createdAt?: Date;
}

export interface HistoryRecord {
  uniqueId: string;
  threadId: string;
  account?: string;
  chatTitle: string;
  questionText: string;
  answerText: string;
  originCode?: string;
  createdAt?: Date;
  turns: HistoryTurn[];
}

export interface ReviseRequest {
  threadId: string;
  chatTitle: string;
  inputText: string;
  account: string;
  employeeId: string;
  originCode: "DDM";
  agentCode: "Local";
}

export interface ReviseResult {
  answer: string | null;
  status: number;
}
