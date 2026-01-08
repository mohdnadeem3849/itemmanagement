import { useEffect, useState } from "react";
import { api } from "../api/apiClient";
import { isAdmin } from "../auth/auth";

export default function ItemsPage() {
  const [items, setItems] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadItems() {
      try {
        const res = await api.get("/api/items");
        setItems(res.data);
      } catch {
        setError("Failed to load items");
      }
    }
    loadItems();
  }, []);

  return (
    <div style={{ padding: 20 }}>
      <h2>Items</h2>

      {error && <p style={{ color: "red" }}>{error}</p>}

      {isAdmin() && <p>You can add items directly (Admin).</p>}

      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>Name</th>
            <th>Description</th>
            <th>Created At</th>
          </tr>
        </thead>
        <tbody>
          {items.map(i => (
            <tr key={i.itemId}>
              <td>{i.name}</td>
              <td>{i.description}</td>
              <td>{new Date(i.createdAt).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
