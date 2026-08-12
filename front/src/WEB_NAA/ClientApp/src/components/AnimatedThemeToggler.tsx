import { Moon, Sun } from "lucide-react";
import { useRef } from "react";
import { flushSync } from "react-dom";

export type AppTheme = "light" | "dark";

interface AnimatedThemeTogglerProps {
  theme: AppTheme;
  onThemeChange: (theme: AppTheme) => void;
}

type ViewTransition = {
  ready: Promise<void>;
};

type ViewTransitionDocument = Document & {
  startViewTransition?: (update: () => void) => ViewTransition;
};

export function AnimatedThemeToggler({
  theme,
  onThemeChange,
}: AnimatedThemeTogglerProps) {
  const buttonRef = useRef<HTMLButtonElement>(null);

  async function toggleTheme() {
    const nextTheme: AppTheme = theme === "dark" ? "light" : "dark";
    const button = buttonRef.current;
    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const startViewTransition = (document as ViewTransitionDocument).startViewTransition;

    if (!button || !startViewTransition || reducedMotion) {
      onThemeChange(nextTheme);
      return;
    }

    const rect = button.getBoundingClientRect();
    const x = rect.left + rect.width / 2;
    const y = rect.top + rect.height / 2;
    const radius = Math.hypot(
      Math.max(x, window.innerWidth - x),
      Math.max(y, window.innerHeight - y),
    );

    const transition = startViewTransition.call(document, () => {
      flushSync(() => onThemeChange(nextTheme));
    });

    await transition.ready;
    document.documentElement.animate(
      {
        clipPath: [
          `circle(0px at ${x}px ${y}px)`,
          `circle(${radius}px at ${x}px ${y}px)`,
        ],
      },
      {
        duration: 460,
        easing: "cubic-bezier(.32,.72,0,1)",
        pseudoElement: "::view-transition-new(root)",
      },
    );
  }

  return (
    <button
      ref={buttonRef}
      type="button"
      className="animated-theme-toggler micro-button micro-button--icon-shift"
      onClick={() => void toggleTheme()}
      aria-label={`外觀：目前為${theme === "dark" ? "深色" : "淺色"}模式`}
    >
      <span className="animated-theme-toggler__icon" aria-hidden="true">
        {theme === "dark" ? <Moon size={18} /> : <Sun size={18} />}
      </span>
      <span>外觀</span>
      <small>{theme === "dark" ? "深色" : "淺色"}</small>
    </button>
  );
}
