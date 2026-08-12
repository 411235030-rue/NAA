import { lazy, Suspense, useLayoutEffect, useState } from "react";
import type { AppTheme } from "./components/AnimatedThemeToggler";
import { NaaIcon } from "./components/NaaIcon";

const ChatPage = lazy(() =>
  import("./components/ChatPage").then((module) => ({ default: module.ChatPage })),
);
const LoginPage = lazy(() =>
  import("./components/LoginPage").then((module) => ({ default: module.LoginPage })),
);

const ACCOUNT_KEY = "naa.account";
const THEME_KEY = "naa.theme";

function readSessionAccount(): string {
  try {
    return window.sessionStorage.getItem(ACCOUNT_KEY)?.trim() || "";
  } catch {
    return "";
  }
}

function readTheme(): AppTheme {
  try {
    const savedTheme = window.localStorage.getItem(THEME_KEY);
    if (savedTheme === "light" || savedTheme === "dark") return savedTheme;
  } catch {
    // The browser can block local storage; system preference remains a safe fallback.
  }
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

export default function App() {
  const [account, setAccount] = useState(readSessionAccount);
  const [theme, setTheme] = useState<AppTheme>(readTheme);

  useLayoutEffect(() => {
    document.documentElement.classList.toggle("dark", theme === "dark");
    document.documentElement.style.colorScheme = theme;
    try {
      window.localStorage.setItem(THEME_KEY, theme);
    } catch {
      // Theme still works for the current page when persistence is unavailable.
    }
  }, [theme]);

  function login(value: string) {
    window.sessionStorage.setItem(ACCOUNT_KEY, value);
    setAccount(value);
  }

  function logout() {
    window.sessionStorage.removeItem(ACCOUNT_KEY);
    setAccount("");
  }

  return (
    <Suspense
      fallback={
        <div className="app-loading" role="status">
          <NaaIcon size={58} aria-hidden="true" />
          <p>正在載入小護天使…</p>
        </div>
      }
    >
      {account ? (
        <ChatPage
          account={account}
          theme={theme}
          onThemeChange={setTheme}
          onLogout={logout}
        />
      ) : (
        <LoginPage onLogin={login} />
      )}
    </Suspense>
  );
}
