import { useEffect, useState } from "react";
import { api } from "../api/apiClient";

export default function AdminRequestsPage() {
  const [requests, setRequests] = useState([]);
  const [error, setError] = useState("");
  const [denyReasonById, setDenyReasonById] = useState({});
  const [msg, setMsg] = useState("");

  async function load() {
    setError("");
    setMsg("");
    try {
      const res = await api.get("/api/admin/requests");
      setRequests(res.data);
    } catch {
      setError("Failed to load admin requests");
    }
  }

  useEffect(() => {
    load();
  }, []);

  async function approve(id) {
    try {
      await api.post(`/api/admin/requests/${id}/approve`);
      setMsg(`Request ${id} approved`);
      load();
    } catch {
      setError("Approve failed");
    }
  }

  async function deny(id) {
    const reason = (denyReasonById[id] || "").trim();
    if (!reason) {
      setError("Denial reason is required.");
      return;
    }

    try {
      await api.post(`/api/admin/requests/${id}/deny`, { reason });
      setMsg(`Request ${id} denied`);
      load();
    } catch {
      setError("Deny failed");
    }
  }

  return (
    <div style={{ padding: 20 }}>
      <h2>Admin Requests</h2>

      {error && <p style={{ color: "crimson" }}>{error}</p>}
      {msg && <p style={{ color: "green" }}>{msg}</p>}

      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>RequestId</th>
            <th>Name</th>
            <th>Description</th>
            <th>Status</th>
            <th>Rejection Reason</th>
            <th>Created At</th>
            <th>Actions</th>
          </tr>
        </thead>

        <tbody>
          {requests.map((r) => (
            <tr key={r.RequestId}>
              <td>{r.RequestId}</td>
              <td>{r.RequestedName}</td>
              <td>{r.RequestedDescription}</td>
              <td>{r.Status}</td>
              <td>{r.RejectionReason || "-"}</td>
              <td>{new Date(r.CreatedAt).toLocaleString()}</td>

              <td>
                <button
                  onClick={() => approve(r.RequestId)}
                  disabled={r.Status !== "Pending" && r.Status !== "Denied"}
                  style={{ marginRight: 8 }}
                >
                  Approve
                </button>

                <div style={{ marginTop: 6 }}>
                  <input
                    placeholder="Denial reason..."
                    value={denyReasonById[r.RequestId] || ""}
                    onChange={(e) =>
                      setDenyReasonById((prev) => ({
                        ...prev,
                        [r.RequestId]: e.target.value,
                      }))
                    }
                    style={{ width: 220, marginRight: 6 }}
                  />
                  <button
                    onClick={() => deny(r.RequestId)}
                    disabled={r.Status !== "Pending"}
                  >
                    Deny
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
