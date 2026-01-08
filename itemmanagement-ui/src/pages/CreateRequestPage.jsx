import { useState } from "react";
import { api } from "../api/apiClient";

export default function CreateRequestPage() {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [msg, setMsg] = useState("");

  async function handleSubmit(e) {
    e.preventDefault();
    setMsg("");

    if (!name) {
      setMsg("Item name is required");
      return;
    }

    try {
      await api.post("/api/requests", {
        requestedName: name,
        requestedDescription: description,
      });
      setMsg("Request submitted successfully");
      setName("");
      setDescription("");
    } catch {
      setMsg("Failed to submit request");
    }
  }

  return (
    <div style={{ padding: 20 }}>
      <h2>Create Item Request</h2>

      <form onSubmit={handleSubmit}>
        <div>
          <label>Name</label><br />
          <input value={name} onChange={e => setName(e.target.value)} />
        </div>

        <div>
          <label>Description</label><br />
          <textarea value={description} onChange={e => setDescription(e.target.value)} />
        </div>

        <button type="submit">Submit Request</button>
      </form>

      {msg && <p>{msg}</p>}
    </div>
  );
}
