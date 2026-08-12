import type { ImgHTMLAttributes } from "react";

interface NaaIconProps extends Omit<ImgHTMLAttributes<HTMLImageElement>, "src"> {
  size?: number;
}

export function NaaIcon({ size = 32, className = "", alt = "", ...props }: NaaIconProps) {
  return (
    <img
      src="/NAAIcon.png"
      width={size}
      height={size}
      className={`naa-icon ${className}`.trim()}
      alt={alt}
      {...props}
    />
  );
}
