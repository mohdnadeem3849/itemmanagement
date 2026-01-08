import { isAdmin, isUser } from "../auth/auth";

export default function Home() {
  return (
    <div style={{ padding: 20 }}>
      <h2>Dashboard</h2>
      {isAdmin() && <p>✅ You are logged in as <b>Admin</b>.</p>}
      {isUser() && <p>✅ You are logged in as <b>User</b>.</p>}
      <p>Next we will add Items + Requests pages.</p>
    </div>
  );
}
