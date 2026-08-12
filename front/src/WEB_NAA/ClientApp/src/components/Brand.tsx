import type { HTMLAttributes } from "react";
import { NaaIcon } from "./NaaIcon";

interface BrandProps extends HTMLAttributes<HTMLDivElement> {
  compact?: boolean;
}

export function Brand({ compact = false, className = "", ...props }: BrandProps) {
  return (
    <div className={`brand ${compact ? "brand--compact" : ""} ${className}`.trim()} {...props}>
      <span className="brand__mark" aria-hidden="true">
        <NaaIcon size={39} />
      </span>
      <span className="brand__copy">
        <strong>小護天使</strong>
        {!compact && <small>NURSING AI ASSISTANT</small>}
      </span>
    </div>
  );
}
