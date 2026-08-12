import { type FormEvent, useEffect, useRef, useState } from "react";
import { ArrowRight, LockKeyhole, UserRound } from "lucide-react";
import GradientWaves from "./GradientWaves";
import { Brand } from "./Brand";
import { GlassCard } from "./GlassCard";
import { LogoLoop, type LogoLoopItem } from "./LogoLoop";

const technologyLogos: readonly LogoLoopItem[] = [
  {
    src: "https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/react/react-original.svg",
    alt: "React",
    title: "React",
    href: "https://react.dev/",
  },
  {
    src: "https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/typescript/typescript-original.svg",
    alt: "TypeScript",
    title: "TypeScript",
    href: "https://www.typescriptlang.org/",
  },
  {
    src: "https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/vitejs/vitejs-original.svg",
    alt: "Vite",
    title: "Vite",
    href: "https://vite.dev/",
  },
  {
    src: "https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/threejs/threejs-original.svg",
    alt: "Three.js",
    title: "Three.js",
    href: "https://threejs.org/",
  },
  {
    src: "https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/csharp/csharp-original.svg",
    alt: "C Sharp",
    title: "C#",
    href: "https://learn.microsoft.com/dotnet/csharp/",
  },
  {
    src: "https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/dotnetcore/dotnetcore-original.svg",
    alt: "ASP.NET Core",
    title: "ASP.NET Core",
    href: "https://dotnet.microsoft.com/apps/aspnet",
  },
  {
    src: "https://www.assistant-ui.com/favicon.ico",
    alt: "assistant-ui",
    title: "assistant-ui",
    href: "https://www.assistant-ui.com/",
  },
  {
    src: "https://reactbits.dev/favicon.ico",
    alt: "React Bits",
    title: "React Bits",
    href: "https://reactbits.dev/",
  },
  {
    src: "https://magicui.design/favicon.ico",
    alt: "Magic UI",
    title: "Magic UI",
    href: "https://magicui.design/",
  },
];

interface LoginPageProps {
  onLogin: (account: string) => void;
}

export function LoginPage({ onLogin }: LoginPageProps) {
  const [account, setAccount] = useState("");
  const [error, setError] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const value = account.trim();
    if (!value) {
      setError("請輸入員工帳號");
      inputRef.current?.focus();
      return;
    }

    setError("");
    onLogin(value);
  }

  return (
    <main className="login-page">
      <div className="login-page__waves" aria-hidden="true">
        <GradientWaves
          horizonColor="#f0ecff"
          waveColor="#db77d8"
          crestColor="#eeabab"
        />
      </div>
      <div className="login-page__veil" aria-hidden="true" />

      <GlassCard className="login-card" aria-labelledby="login-title">
        <Brand className="login-card__brand" />

        <form className="login-form" onSubmit={submit} noValidate>
          <div className="login-form__eyebrow">WELCOME BACK</div>
          <h1 id="login-title">歡迎回來</h1>
          <p>使用員工帳號登入，繼續你的護理 AI 對話。</p>

          <label htmlFor="account">員工帳號</label>
          <div className={`login-input ${error ? "login-input--error" : ""}`}>
            <UserRound size={20} aria-hidden="true" />
            <input
              ref={inputRef}
              id="account"
              name="account"
              type="text"
              value={account}
              onChange={(event) => {
                setAccount(event.target.value);
                if (error) setError("");
              }}
              autoComplete="username"
              placeholder="例如：N001"
              aria-describedby={error ? "account-error" : undefined}
              aria-invalid={Boolean(error)}
            />
          </div>

          {error && (
            <div id="account-error" className="login-form__error" role="alert">
              {error}
            </div>
          )}

          <button className="login-button" type="submit">
            <span>進入系統</span>
            <ArrowRight size={20} aria-hidden="true" />
          </button>

          <div className="login-note">
            <LockKeyhole size={17} aria-hidden="true" />
            <span>帳號僅用於載入與保存你的個人歷史紀錄。</span>
          </div>
        </form>

        <footer>NAA · Nursing AI Assistant</footer>
      </GlassCard>

      <div className="login-tech" aria-label="本系統使用技術">
        <span className="login-tech__label">BUILT WITH</span>
        <LogoLoop
          logos={technologyLogos}
          speed={42}
          logoHeight={30}
          gap={58}
          pauseOnHover
          fadeOut
          scaleOnHover
          ariaLabel="本系統使用技術商標"
        />
      </div>
    </main>
  );
}
