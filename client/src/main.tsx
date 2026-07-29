import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import "bootstrap/dist/css/bootstrap.min.css";

const setPwaViewportZoomLock = () => {
  const isStandalonePwa =
    window.matchMedia("(display-mode: standalone)").matches ||
    (window.navigator as Navigator & { standalone?: boolean }).standalone ===
      true;
  const isTouchDevice =
    window.matchMedia("(pointer: coarse)").matches ||
    navigator.maxTouchPoints > 0;

  let viewport = document.querySelector<HTMLMetaElement>(
    'meta[name="viewport"]',
  );

  if (!viewport) {
    viewport = document.createElement("meta");
    viewport.name = "viewport";
    document.head.appendChild(viewport);
  }

  viewport.content =
    isStandalonePwa && isTouchDevice
      ? "width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"
      : "width=device-width, initial-scale=1.0";
};

setPwaViewportZoomLock();

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
);
