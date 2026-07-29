import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { AdminUser } from "../Data";
import {
  fetchAllUsers,
  toggleUserApproval,
  deleteUser,
  isAdmin,
} from "../services/UserService";
import ImageDisplay from "./ImageDisplay";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

const ManageUsers: React.FC = () => {
  const navigate = useNavigate();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAdmin()) {
      navigate("/mymdb/media");
      return;
    }

    const loadUsers = async () => {
      try {
        const fetchedUsers = await fetchAllUsers();
        setUsers(fetchedUsers || []);
        setLoading(false);
      } catch (err: any) {
        console.error("Failed to load users:", err);
        setError(err?.response?.data || "Error loading users");
        setLoading(false);
      }
    };

    loadUsers();
  }, [navigate]);

  const handleToggleApproval = async (user: AdminUser) => {
    try {
      await toggleUserApproval(user.id, !user.approved);
      setUsers(
        users.map((u) =>
          u.id === user.id ? { ...u, approved: !u.approved } : u,
        ),
      );
    } catch (err) {
      setError("Error updating user approval");
    }
  };

  const handleDeleteUser = async (user: AdminUser) => {
    const confirmed = window.confirm(
      `Are you sure you want to delete user "${user.email}"? This action cannot be undone.`,
    );
    if (!confirmed) return;

    try {
      await deleteUser(user.id);
      setUsers(users.filter((u) => u.id !== user.id));
    } catch (err: any) {
      const message =
        err.response?.data ||
        err.response?.data?.message ||
        "Error deleting user";
      setError(message);
    }
  };

  if (loading) {
    return <div className="p-5">Loading...</div>;
  }

  return (
    <div className="p-4 p-md-5">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Manage Users</h2>
        <button
          className="btn btn-secondary"
          onClick={() => navigate("/mymdb/media")}
        >
          Back to Media
        </button>
      </div>

      {error && (
        <div className="alert alert-danger alert-dismissible" role="alert">
          {error}
          <button
            type="button"
            className="btn-close"
            onClick={() => setError(null)}
          ></button>
        </div>
      )}

      {users.length === 0 ? (
        <div className="alert alert-info" role="alert">
          No users found.
        </div>
      ) : (
        <div className="table-responsive">
          <table className="table table-dark table-hover">
            <thead>
              <tr>
                <th scope="col" style={{ width: "60px" }}>
                  Photo
                </th>
                <th scope="col">Name</th>
                <th scope="col">Email</th>
                <th scope="col">Role</th>
                <th scope="col" style={{ width: "100px" }}>
                  Approved
                </th>
                <th scope="col" style={{ width: "100px" }}>
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>
                    <div
                      style={{
                        width: "40px",
                        height: "40px",
                        borderRadius: "50%",
                        overflow: "hidden",
                        backgroundColor: "#333",
                      }}
                    >
                      {user.profilePicPath ? (
                        <ImageDisplay
                          src={user.profilePicPath}
                          alt={user.userName || user.email || "User"}
                          style={{
                            width: "100%",
                            height: "100%",
                            objectFit: "cover",
                          }}
                        />
                      ) : (
                        <div
                          style={{
                            width: "100%",
                            height: "100%",
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            color: "#888",
                            fontSize: "18px",
                          }}
                        >
                          👤
                        </div>
                      )}
                    </div>
                  </td>
                  <td className="align-middle">
                    {user.userName || <span className="text-muted">N/A</span>}
                  </td>
                  <td className="align-middle">{user.email}</td>
                  <td className="align-middle">
                    {user.roles?.includes("admin") ? (
                      <span className="badge bg-danger">Admin</span>
                    ) : (
                      <span className="badge bg-secondary">User</span>
                    )}
                  </td>
                  <td className="align-middle">
                    <div className="form-check form-switch">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        role="switch"
                        checked={user.approved}
                        onChange={() => handleToggleApproval(user)}
                        disabled={user.roles?.includes("admin")}
                        style={{
                          cursor: user.roles?.includes("admin")
                            ? "not-allowed"
                            : "pointer",
                        }}
                      />
                    </div>
                  </td>
                  <td className="align-middle">
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => handleDeleteUser(user)}
                      disabled={user.roles?.includes("admin")}
                      title={
                        user.roles?.includes("admin")
                          ? "Cannot delete admin users"
                          : "Delete user"
                      }
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default ManageUsers;
