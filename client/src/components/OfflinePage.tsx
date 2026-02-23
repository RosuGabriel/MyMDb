export default function OfflinePage({
  type,
}: {
  type: "no-internet" | "server-down";
}) {
  const messages = {
    "no-internet": "No internet connection 😞",
    "server-down": "Server is down 😕",
  };

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        alignItems: "center",
        height: "100vh",
        textAlign: "center",
        backgroundColor: "#212529",
        color: "#f8f9fa",
        fontFamily: "Arial, sans-serif",
        padding: "1rem",
      }}
    >
      <img src="/redpanda_bgless.png" alt="App Icon" width={100} height={100} />
      <br />
      <br />
      <h1>{messages[type]}</h1>
      <p>Check your connection or try again later.</p>
    </div>
  );
}
