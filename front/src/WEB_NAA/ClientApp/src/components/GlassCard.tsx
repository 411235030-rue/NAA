import type { CSSProperties, HTMLAttributes, PointerEvent } from "react";

type GlassCardStyle = CSSProperties & {
  "--glass-pointer-x"?: string;
  "--glass-pointer-y"?: string;
};

export function GlassCard({
  className = "",
  children,
  style,
  onPointerMove,
  onPointerLeave,
  ...props
}: HTMLAttributes<HTMLElement>) {
  function updateGlow(event: PointerEvent<HTMLElement>) {
    const bounds = event.currentTarget.getBoundingClientRect();
    event.currentTarget.style.setProperty(
      "--glass-pointer-x",
      `${event.clientX - bounds.left}px`,
    );
    event.currentTarget.style.setProperty(
      "--glass-pointer-y",
      `${event.clientY - bounds.top}px`,
    );
    onPointerMove?.(event);
  }

  function resetGlow(event: PointerEvent<HTMLElement>) {
    event.currentTarget.style.removeProperty("--glass-pointer-x");
    event.currentTarget.style.removeProperty("--glass-pointer-y");
    onPointerLeave?.(event);
  }

  return (
    <section
      className={`glass-card ${className}`.trim()}
      style={style as GlassCardStyle}
      onPointerMove={updateGlow}
      onPointerLeave={resetGlow}
      {...props}
    >
      <span className="glass-card__noise" aria-hidden="true" />
      <span className="glass-card__shine" aria-hidden="true" />
      <div className="glass-card__content">{children}</div>
    </section>
  );
}
