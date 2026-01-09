import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/apiClient";
import { saveAuth } from "../auth/auth";

export default function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  async function handleLogin(e) {
    e.preventDefault();
    setError("");

    const u = username.trim();
    const p = password.trim();

    if (!u || !p) {
      setError("Username and password are required.");
      return;
    }

    // ✅ DEBUG (temporary - keep until it works)
    console.log("API BASE:", api.defaults.baseURL);
    console.log("LOGIN PAYLOAD:", { username: u, passwordLength: p.length });

    try {
      // ✅ Important: send trimmed values
      const res = await api.post("/api/auth/login", { username: u, password: p });

      // ✅ Save token + roles
      saveAuth(res.data);

      // ✅ Go to Home
      navigate("/");
    } catch (err) {
      // ✅ Show real error
      console.log("LOGIN ERROR FULL:", err);
      console.log("STATUS:", err?.response?.status);
      console.log("DATA:", err?.response?.data);

      const data = err?.response?.data;

      // If API returns plain text
      if (typeof data === "string" && data.trim()) {
        setError(data);
        return;
      }

      // If API returns JSON like { message: "..." }
      if (data?.message) {
        setError(data.message);
        return;
      }

      setError("Login failed (check username/password)");
    }
  }

  return (
    <div
      style={{
        maxWidth: 360,
        margin: "60px auto",
        padding: 16,
        border: "1px solid #ddd",
        borderRadius: 8,
      }}
    >
      <h2 style={{ marginTop: 0 }}>Login</h2>

      <form onSubmit={handleLogin}>
        <div style={{ marginBottom: 10 }}>
          <label>Username</label>
          <input
            style={{ width: "100%", padding: 8 }}
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            autoComplete="username"
          />
        </div>

        <div style={{ marginBottom: 10 }}>
          <label>Password</label>
          <input
            style={{ width: "100%", padding: 8 }}
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
          />
        </div>

        {error && <p style={{ color: "crimson" }}>{error}</p>}

        <button type="submit" style={{ width: "100%", padding: 10 }}>
          Login
        </button>
      </form>
    </div>
  );
}
