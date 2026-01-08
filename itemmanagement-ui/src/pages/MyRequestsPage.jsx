import { useEffect, useState } from "react";
import { api } from "../api/apiClient";

export default function MyRequestsPage() {
  const [requests, setRequests] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    async function load() {
      try {
        const res = await api.get("/api/requests/my");
        setRequests(res.data);
      } catch {
        setError("Failed to load requests");
      }
    }
    load();
  }, []);

  return (
    <div style={{ padding: 20 }}>
      <h2>My Requests</h2>

      {error && <p style={{ color: "red" }}>{error}</p>}

      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>Name</th>
            <th>Description</th>
            <th>Status</th>
            <th>Rejection Reason</th>
            <th>Created At</th>
          </tr>
        </thead>
        <tbody>
          {requests.map(r => (
            <tr key={r.requestId}>
              <td>{r.requestedName}</td>
              <td>{r.requestedDescription}</td>
              <td>{r.status}</td>
              <td>{r.rejectionReason || "-"}</td>
              <td>{new Date(r.createdAt).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
