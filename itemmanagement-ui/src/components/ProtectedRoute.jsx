import { Navigate } from "react-router-dom";
import { isLoggedIn, getRoles } from "../auth/auth";

export default function ProtectedRoute({ children, roles }) {
  if (!isLoggedIn()) return <Navigate to="/login" replace />;

  if (roles && roles.length > 0) {
    const userRoles = getRoles();
    const allowed = roles.some((r) => userRoles.includes(r));
    if (!allowed) return <Navigate to="/forbidden" replace />;
  }

  return children;
}
