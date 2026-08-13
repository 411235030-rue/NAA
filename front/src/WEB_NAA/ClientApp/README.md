# 小護天使 React 前端

這是 `WEB_NAA` 的 React + TypeScript + Vite 遷移版本。聊天元件使用 assistant-ui，登入背景沿用 React Bits 的 Liquid Ether，既有 DDM API 合約保持不變。

## 已保留的功能

- 員工帳號登入與登出
- 互動式 Liquid Ether 登入背景
- DDM `/ReviseText` 問答
- 依帳號查詢 `/GetHistoryByAccount`
- 最近對話、歷史對話切換與開始新對話
- Gemini 風格桌面與行動版介面

## 本機啟動

先啟動既有 API（5210）與 DDM（7079），再於本資料夾執行：

```powershell
pnpm install
pnpm dev
```

ASP.NET Core 前端網址為 `https://localhost:5124`，並把 `/ddm` 代理到 `https://localhost:7079`，因此不需要修改 DDM 的 CORS。

若 DDM 使用其他網址，複製 `.env.example` 為 `.env.local` 並修改 `DDM_PROXY_TARGET`。

## 建置

```powershell
pnpm build
```

輸出位於 `dist`。正式環境需將 `/ddm/*` 反向代理至 DDM，或設定 `VITE_DDM_BASE_URL` 並在 DDM 啟用對應 CORS。

## 重要說明

目前登入機制與原 Blazor 版本一致，只使用員工帳號識別歷史紀錄，並不是密碼或企業身分驗證。若系統將對外開放，應由 ASP.NET Core 後端補上正式驗證與授權。
