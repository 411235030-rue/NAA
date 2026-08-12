import {
  AssistantRuntimeProvider,
  type AppendMessage,
  type ThreadMessageLike,
  useExternalStoreRuntime,
} from "@assistant-ui/react";
import {
  ArrowLeft,
  BookOpenText,
  ChevronUp,
  CircleUserRound,
  LogOut,
  Menu,
  MessageSquarePlus,
  PanelLeftClose,
  PanelLeftOpen,
  RefreshCw,
  Settings,
  X,
} from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { getHistoryByAccount, reviseText } from "../api/ddm";
import type { ChatMessage, HistoryRecord } from "../types";
import {
  AnimatedThemeToggler,
  type AppTheme,
} from "./AnimatedThemeToggler";
import { AssistantThread } from "./AssistantThread";
import { Brand } from "./Brand";
import { NaaIcon } from "./NaaIcon";

interface ChatPageProps {
  account: string;
  theme: AppTheme;
  onThemeChange: (theme: AppTheme) => void;
  onLogout: () => void;
}

function createId(prefix: string): string {
  if (typeof crypto.randomUUID === "function") return `${prefix}-${crypto.randomUUID()}`;
  return `${prefix}-${Date.now()}-${Math.random().toString(36).slice(2)}`;
}

function readAppendText(message: AppendMessage): string {
  return message.content
    .filter((part): part is Extract<(typeof message.content)[number], { type: "text" }> => part.type === "text")
    .map((part) => part.text)
    .join("\n")
    .trim();
}

function historyMessages(record: HistoryRecord): ChatMessage[] {
  const result: ChatMessage[] = [];
  record.turns.forEach((turn, index) => {
    if (turn.questionText.trim()) {
      result.push({
        id: turn.uniqueId ? `${turn.uniqueId}-q` : `${record.uniqueId}-${index}-q`,
        role: "user",
        text: turn.questionText,
        createdAt: turn.createdAt || new Date(),
      });
    }
    if (turn.answerText.trim()) {
      result.push({
        id: turn.uniqueId ? `${turn.uniqueId}-a` : `${record.uniqueId}-${index}-a`,
        role: "assistant",
        text: turn.answerText,
        createdAt: turn.createdAt || new Date(),
      });
    }
  });
  return result;
}

function formatHistoryTime(date?: Date): string {
  if (!date) return "歷史紀錄";
  const today = new Date();
  if (date.toDateString() === today.toDateString()) {
    return date.toLocaleTimeString("zh-TW", { hour: "2-digit", minute: "2-digit" });
  }
  const yesterday = new Date(today);
  yesterday.setDate(today.getDate() - 1);
  if (date.toDateString() === yesterday.toDateString()) return "昨天";
  return date.toLocaleDateString("zh-TW", { month: "2-digit", day: "2-digit" });
}

export function ChatPage({ account, theme, onThemeChange, onLogout }: ChatPageProps) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [history, setHistory] = useState<HistoryRecord[]>([]);
  const [selectedHistoryId, setSelectedHistoryId] = useState<string>();
  const [threadId, setThreadId] = useState(() => createId("thread"));
  const [isRunning, setIsRunning] = useState(false);
  const [isHistoryLoading, setIsHistoryLoading] = useState(true);
  const [historyError, setHistoryError] = useState("");
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [accountMenuOpen, setAccountMenuOpen] = useState(false);
  const [accountPanel, setAccountPanel] = useState<"menu" | "settings" | "help">("menu");
  const accountMenuRef = useRef<HTMLDivElement>(null);

  const closeAccountMenu = useCallback(() => {
    setAccountMenuOpen(false);
    setAccountPanel("menu");
  }, []);

  const refreshHistory = useCallback(async () => {
    setIsHistoryLoading(true);
    setHistoryError("");
    try {
      setHistory(await getHistoryByAccount(account));
    } catch (error) {
      setHistoryError(error instanceof Error ? error.message : "暫時無法載入歷史紀錄。");
    } finally {
      setIsHistoryLoading(false);
    }
  }, [account]);

  useEffect(() => {
    void refreshHistory();
  }, [refreshHistory]);

  useEffect(() => {
    if (!accountMenuOpen) return;

    const closeOnOutsideClick = (event: PointerEvent) => {
      if (!accountMenuRef.current?.contains(event.target as Node)) closeAccountMenu();
    };
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape") closeAccountMenu();
    };

    document.addEventListener("pointerdown", closeOnOutsideClick);
    document.addEventListener("keydown", closeOnEscape);
    return () => {
      document.removeEventListener("pointerdown", closeOnOutsideClick);
      document.removeEventListener("keydown", closeOnEscape);
    };
  }, [accountMenuOpen, closeAccountMenu]);

  const startNewChat = useCallback(() => {
    setMessages([]);
    setSelectedHistoryId(undefined);
    setThreadId(createId("thread"));
    setSidebarOpen(false);
    closeAccountMenu();
  }, [closeAccountMenu]);

  const openHistory = useCallback((item: HistoryRecord) => {
    setMessages(historyMessages(item));
    setSelectedHistoryId(item.uniqueId);
    setThreadId(item.threadId || item.uniqueId);
    setSidebarOpen(false);
    closeAccountMenu();
  }, [closeAccountMenu]);

  const onNew = useCallback(
    async (incoming: AppendMessage) => {
      const text = readAppendText(incoming);
      if (!text || isRunning) return;

      const userMessage: ChatMessage = {
        id: createId("user"),
        role: "user",
        text,
        createdAt: new Date(),
      };

      setMessages((current) => [...current, userMessage]);
      setSelectedHistoryId(undefined);
      setIsRunning(true);

      try {
        const response = await reviseText({
          threadId,
          chatTitle: "小護天使",
          inputText: text,
          account,
          employeeId: account,
          originCode: "DDM",
          agentCode: "Local",
        });

        setMessages((current) => [
          ...current,
          {
            id: createId("assistant"),
            role: "assistant",
            text:
              response.answer ||
              `DDM 已成功收到請求（HTTP ${response.status}），目前沒有 AI 回覆內容。`,
            createdAt: new Date(),
            tone: response.answer ? "default" : "success",
          },
        ]);
        await refreshHistory();
      } catch (error) {
        setMessages((current) => [
          ...current,
          {
            id: createId("error"),
            role: "assistant",
            text: error instanceof Error ? error.message : "DDM 請求失敗，請稍後再試。",
            createdAt: new Date(),
            tone: "error",
          },
        ]);
      } finally {
        setIsRunning(false);
      }
    },
    [account, isRunning, refreshHistory, threadId],
  );

  const convertMessage = useCallback(
    (message: ChatMessage): ThreadMessageLike => ({
      id: message.id,
      role: message.role,
      content: [{ type: "text", text: message.text }],
      createdAt: message.createdAt,
      status: message.role === "assistant" ? { type: "complete", reason: "stop" } : undefined,
      metadata: { custom: { tone: message.tone || "default" } },
    }),
    [],
  );

  const runtime = useExternalStoreRuntime({
    messages,
    isRunning,
    onNew,
    convertMessage,
  });

  const initial = useMemo(() => account.trim().charAt(0).toUpperCase() || "護", [account]);

  return (
    <AssistantRuntimeProvider runtime={runtime}>
      <div className={`app-shell ${sidebarCollapsed ? "app-shell--sidebar-collapsed" : ""}`}>
        <aside
          className={`sidebar ${sidebarOpen ? "sidebar--open" : ""} ${
            sidebarCollapsed ? "sidebar--collapsed" : ""
          }`}
        >
          <div className="sidebar__desktop-head">
            <Brand className="sidebar__brand" />
            <button
              type="button"
              className="sidebar__toggle micro-button micro-button--icon-shift"
              onClick={() => setSidebarCollapsed((collapsed) => !collapsed)}
              aria-label={sidebarCollapsed ? "展開側欄" : "收合側欄"}
              title={sidebarCollapsed ? "展開側欄" : "收合側欄"}
            >
              {sidebarCollapsed ? <PanelLeftOpen size={20} /> : <PanelLeftClose size={20} />}
            </button>
          </div>

          <div className="sidebar__mobile-head">
            <Brand />
            <button className="micro-button" type="button" onClick={() => setSidebarOpen(false)} aria-label="關閉側欄">
              <X size={20} />
            </button>
          </div>

          <button type="button" className="new-chat-button micro-button micro-button--glare" onClick={startNewChat}>
            <MessageSquarePlus size={19} />
            <span>開始新對話</span>
          </button>

          <div className="sidebar__history">
            <div className="history-heading">
              <span>最近對話</span>
              <button
                type="button"
                className="micro-button micro-button--rotate"
                onClick={() => void refreshHistory()}
                disabled={isHistoryLoading}
                aria-label="重新整理歷史紀錄"
              >
                <RefreshCw className={isHistoryLoading ? "is-spinning" : ""} size={15} />
              </button>
            </div>

            <div className="history-panel">
              {isHistoryLoading ? (
                <div className="history-state">
                  <span className="history-state__spinner" />
                  <span>載入歷史紀錄…</span>
                </div>
              ) : historyError ? (
                <div className="history-state history-state--error">
                  <strong>暫時無法載入</strong>
                  <span>{historyError}</span>
                </div>
              ) : history.length === 0 ? (
                <div className="history-state">
                  <NaaIcon size={30} />
                  <strong>尚無對話紀錄</strong>
                  <span>完成一組問答後，紀錄會顯示在這裡。</span>
                </div>
              ) : (
                <div className="history-list">
                  {history.slice(0, 20).map((item) => (
                    <button
                      key={item.uniqueId}
                      type="button"
                      className={`micro-button ${selectedHistoryId === item.uniqueId ? "is-selected" : ""}`}
                      onClick={() => openHistory(item)}
                      title={item.questionText}
                    >
                      <strong>{item.chatTitle}</strong>
                      <span>{item.questionText || "已保存的對話"}</span>
                      <small>{formatHistoryTime(item.createdAt)}</small>
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>

          <div className="sidebar-account" ref={accountMenuRef}>
            {accountMenuOpen && (
              <div className="sidebar-account__popover" role="dialog" aria-label="帳號選單">
                {accountPanel === "menu" ? (
                  <>
                    <div className="sidebar-account__summary">
                      <span className="sidebar-account__avatar">{initial}</span>
                      <span>
                        <strong>{account}</strong>
                        <small>護理同仁</small>
                      </span>
                    </div>
                    <div className="sidebar-account__divider" />
                    <button className="micro-button micro-button--icon-shift" type="button" onClick={() => setAccountPanel("settings")}>
                      <Settings size={18} />
                      <span>個人設定</span>
                    </button>
                    <button className="micro-button micro-button--icon-shift" type="button" onClick={() => setAccountPanel("help")}>
                      <BookOpenText size={18} />
                      <span>使用說明</span>
                    </button>
                    <AnimatedThemeToggler theme={theme} onThemeChange={onThemeChange} />
                    <div className="sidebar-account__divider" />
                    <button type="button" className="sidebar-account__logout micro-button micro-button--icon-shift" onClick={onLogout}>
                      <LogOut size={18} />
                      <span>登出</span>
                    </button>
                  </>
                ) : (
                  <>
                    <div className="sidebar-account__panel-head">
                      <button className="micro-button" type="button" onClick={() => setAccountPanel("menu")} aria-label="返回帳號選單">
                        <ArrowLeft size={18} />
                      </button>
                      <strong>{accountPanel === "settings" ? "個人設定" : "使用說明"}</strong>
                    </div>
                    {accountPanel === "settings" ? (
                      <div className="sidebar-account__details">
                        <CircleUserRound size={26} />
                        <dl>
                          <div><dt>員工帳號</dt><dd>{account}</dd></div>
                          <div><dt>使用身分</dt><dd>護理同仁</dd></div>
                        </dl>
                      </div>
                    ) : (
                      <ol className="sidebar-account__help">
                        <li>點擊「開始新對話」建立新聊天。</li>
                        <li>從最近對話開啟過往問答紀錄。</li>
                        <li>在下方輸入問題並送出給小護天使。</li>
                      </ol>
                    )}
                  </>
                )}
              </div>
            )}
            <button
              type="button"
              className="sidebar-account__trigger micro-button"
              onClick={() => {
                if (!accountMenuOpen) setAccountPanel("menu");
                setAccountMenuOpen((open) => !open);
              }}
              aria-expanded={accountMenuOpen}
              aria-haspopup="dialog"
              title={sidebarCollapsed ? account : undefined}
            >
              <span className="sidebar-account__avatar">{initial}</span>
              <span className="sidebar-account__copy">
                <strong>{account}</strong>
                <small>護理同仁</small>
              </span>
              <ChevronUp className="sidebar-account__chevron" size={16} />
            </button>
          </div>
        </aside>

        {sidebarOpen && (
          <button
            type="button"
            className="sidebar-backdrop"
            onClick={() => setSidebarOpen(false)}
            aria-label="關閉側欄"
          />
        )}

        <main className="workspace">
          <button
            type="button"
            className="workspace__sidebar-open micro-button micro-button--icon-shift"
            onClick={() => setSidebarOpen(true)}
            aria-label="開啟對話選單"
          >
            <Menu size={21} />
          </button>

          <section className="workspace__content">
            <AssistantThread account={account} />
          </section>
        </main>
      </div>
    </AssistantRuntimeProvider>
  );
}
