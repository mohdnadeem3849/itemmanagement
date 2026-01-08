import { Link, Route, Routes, useNavigate } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import Home from "./pages/Home";
import Forbidden from "./pages/Forbidden";
import ItemsPage from "./pages/ItemsPage";
import CreateRequestPage from "./pages/CreateRequestPage";
import MyRequestsPage from "./pages/MyRequestsPage";
import AdminRequestsPage from "./pages/AdminRequestsPage";
import ProtectedRoute from "./components/ProtectedRoute";
import { isLoggedIn, isAdmin, isUser, logout } from "./auth/auth";

export default function App() {
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <div>
      <nav style={{ padding: 12, borderBottom: "1px solid #ddd" }}>
        {isLoggedIn() ? (
          <>
            <Link to="/">Home</Link>{" | "}
            <Link to="/items">Items</Link>{" | "}

            {isUser() && (
              <>
                <Link to="/my-requests">My Requests</Link>{" | "}
                <Link to="/create-request">Create Request</Link>{" | "}
              </>
            )}

            {isAdmin() && (
              <>
                <Link to="/admin/requests">Admin Requests</Link>{" | "}
                <Link to="/admin/users">Admin Users</Link>{" | "}
              </>
            )}

            <button onClick={handleLogout} style={{ marginLeft: 10 }}>
              Logout
            </button>
          </>
        ) : (
          <Link to="/login">Login</Link>
        )}
      </nav>

      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/forbidden" element={<Forbidden />} />

        <Route
          path="/"
          element={
            <ProtectedRoute>
              <Home />
            </ProtectedRoute>
          }
        />

        {/* ✅ Items Page (User + Admin) */}
        <Route
          path="/items"
          element={
            <ProtectedRoute>
              <ItemsPage />
            </ProtectedRoute>
          }
        />

        {/* ✅ My Requests (User only) */}
        <Route
          path="/my-requests"
          element={
            <ProtectedRoute roles={["User"]}>
              <MyRequestsPage />
            </ProtectedRoute>
          }
        />

        {/* ✅ Create Request (User only) */}
        <Route
          path="/create-request"
          element={
            <ProtectedRoute roles={["User"]}>
              <CreateRequestPage />
            </ProtectedRoute>
          }
        />

        {/* ✅ Admin Requests (Admin only) */}
        <Route
          path="/admin/requests"
          element={
            <ProtectedRoute roles={["Admin"]}>
              <AdminRequestsPage />
            </ProtectedRoute>
          }
        />

        <Route path="*" element={<div style={{ padding: 20 }}>Not Found</div>} />
      </Routes>
    </div>
  );
}
