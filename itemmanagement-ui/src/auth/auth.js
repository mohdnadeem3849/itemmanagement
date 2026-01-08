export function saveAuth(data) {
  localStorage.setItem("token", data.token);
  localStorage.setItem("userId", data.userId);
  localStorage.setItem("username", data.username);
  localStorage.setItem("roles", JSON.stringify(data.roles || []));
}

export function logout() {
  localStorage.clear();
}

export function isLoggedIn() {
  return !!localStorage.getItem("token");
}

export function getRoles() {
  return JSON.parse(localStorage.getItem("roles") || "[]");
}

export function isAdmin() {
  return getRoles().includes("Admin");
}

export function isUser() {
  return getRoles().includes("User");
}
