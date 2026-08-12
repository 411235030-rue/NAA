# WEB_NAA

目前前端已改為 React + TypeScript + Vite，ASP.NET Core 專案只負責：

1. 提供 `wwwroot` 內的 React 正式建置檔。
2. 將 `/ddm/ReviseText` 與 `/ddm/GetHistoryByAccount` 代理至 DDM。
3. 讓 Visual Studio 能沿用 `WEB_NAA.slnx` 直接啟動系統。

## Visual Studio 啟動

請先啟動 API（5210）與 DDM（7079），再以 `http` 設定檔執行 `WEB_NAA`。瀏覽器會開啟 `http://localhost:5124/login`。

## 修改 React 前端

React 原始碼位於 `ClientApp`：

```powershell
pnpm install
pnpm dev
```

完成修改後執行：

```powershell
pnpm build
```

再將 `ClientApp/dist` 內容同步到 `wwwroot`，即可由 ASP.NET Core 提供最新正式版本。

舊 Blazor 程式已保存於 `front/backups/WEB_NAA-Blazor-20260812`。
