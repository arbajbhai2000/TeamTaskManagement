import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../services/api";
import "./Users.css";

function Users() {
  const navigate = useNavigate();

  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);

  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState("All");

  // Create / Edit modal
  const [showUserModal, setShowUserModal] = useState(false);
  const [editingUser, setEditingUser] = useState(null);

  const [saving, setSaving] = useState(false);
  const [modalError, setModalError] = useState("");
  const [modalSuccess, setModalSuccess] = useState("");

  const [form, setForm] = useState({
    name: "",
    email: "",
    password: "",
    role: "2",
  });

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const usersPerPage = 10;

  // Page message
  const [pageMessage, setPageMessage] = useState("");

  // ---------------------------------------------------------
  // LOAD USERS
  // ---------------------------------------------------------

  const loadUsers = async (showLoader = true) => {
    try {
      if (showLoader) {
        setLoading(true);
      }

      const response = await api.get("/Users");

      setUsers(response.data || []);
    } catch (err) {
      console.error("Failed to load users:", err);
    } finally {
      if (showLoader) {
        setLoading(false);
      }
    }
  };

  useEffect(() => {
    const token = localStorage.getItem("accessToken");

    if (!token) {
      navigate("/login");
      return;
    }

    loadUsers();
  }, [navigate]);

  // ---------------------------------------------------------
  // FILTER
  // ---------------------------------------------------------

  const filteredUsers = useMemo(() => {
    const searchText = search.trim().toLowerCase();

    return users.filter((user) => {
      const matchesSearch =
        !searchText ||
        user.name?.toLowerCase().includes(searchText) ||
        user.email?.toLowerCase().includes(searchText);

      const roleName = getRoleName(user.role);

      const matchesRole =
        roleFilter === "All" ||
        roleName === roleFilter;

      return matchesSearch && matchesRole;
    });
  }, [users, search, roleFilter]);

  // ---------------------------------------------------------
  // PAGINATION
  // ---------------------------------------------------------

  const totalPages = Math.max(
    1,
    Math.ceil(filteredUsers.length / usersPerPage)
  );

  const paginatedUsers = useMemo(() => {
    const startIndex =
      (currentPage - 1) * usersPerPage;

    return filteredUsers.slice(
      startIndex,
      startIndex + usersPerPage
    );
  }, [filteredUsers, currentPage]);

  useEffect(() => {
    setCurrentPage(1);
  }, [search, roleFilter]);

  useEffect(() => {
    if (currentPage > totalPages) {
      setCurrentPage(totalPages);
    }
  }, [currentPage, totalPages]);

  // ---------------------------------------------------------
  // HELPERS
  // ---------------------------------------------------------

  function getInitials(name) {
    if (!name) return "U";

    return name
      .split(" ")
      .filter(Boolean)
      .slice(0, 2)
      .map((part) =>
        part.charAt(0).toUpperCase()
      )
      .join("");
  }

  function getRoleName(role) {
    if (typeof role === "string") {
      return role;
    }

    switch (Number(role)) {
      case 0:
        return "Admin";

      case 1:
        return "Manager";

      case 2:
        return "User";

      default:
        return "User";
    }
  }

  function getRoleClass(role) {
    const roleName = getRoleName(role);

    if (roleName === "Admin") {
      return "role-admin";
    }

    if (roleName === "Manager") {
      return "role-manager";
    }

    return "role-user";
  }

  function getRoleValue(role) {
    const roleName = getRoleName(role);

    if (roleName === "Admin") return "0";
    if (roleName === "Manager") return "1";

    return "2";
  }

  // ---------------------------------------------------------
  // CREATE
  // ---------------------------------------------------------

  const openCreateModal = () => {
    setEditingUser(null);

    setForm({
      name: "",
      email: "",
      password: "",
      role: "2",
    });

    setModalError("");
    setModalSuccess("");

    setShowUserModal(true);
  };

  // ---------------------------------------------------------
  // EDIT
  // ---------------------------------------------------------

  const openEditModal = (user) => {
    setEditingUser(user);

    setForm({
      name: user.name || "",
      email: user.email || "",
      password: "",
      role: getRoleValue(user.role),
    });

    setModalError("");
    setModalSuccess("");

    setShowUserModal(true);
  };

  // ---------------------------------------------------------
  // CLOSE MODAL
  // ---------------------------------------------------------

  const closeUserModal = () => {
    if (saving) return;

    setShowUserModal(false);
    setEditingUser(null);

    setForm({
      name: "",
      email: "",
      password: "",
      role: "2",
    });

    setModalError("");
    setModalSuccess("");
  };

  // ---------------------------------------------------------
  // FORM CHANGE
  // ---------------------------------------------------------

  const handleFormChange = (event) => {
    const { name, value } = event.target;

    setForm((previous) => ({
      ...previous,
      [name]: value,
    }));

    setModalError("");
    setModalSuccess("");
  };

  // ---------------------------------------------------------
  // CREATE / UPDATE
  // ---------------------------------------------------------

  const handleSubmit = async (event) => {
    event.preventDefault();

    setModalError("");
    setModalSuccess("");

    const name = form.name.trim();
    const email = form.email.trim();

    if (!name) {
      setModalError("Name is required.");
      return;
    }

    if (!email) {
      setModalError("Email is required.");
      return;
    }

    if (
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
    ) {
      setModalError(
        "Please enter a valid email address."
      );
      return;
    }

    if (!editingUser) {
      if (!form.password) {
        setModalError("Password is required.");
        return;
      }

      if (form.password.length < 6) {
        setModalError(
          "Password must be at least 6 characters."
        );
        return;
      }
    }

    try {
      setSaving(true);

      if (editingUser) {
        // UPDATE USER

        await api.put(
          `/Users/${editingUser.id}`,
          {
            name,
            email,
            role: Number(form.role),
          }
        );

        setModalSuccess(
          "User updated successfully."
        );
      } else {
        // CREATE USER

        await api.post("/Users", {
          name,
          email,
          password: form.password,
          role: Number(form.role),
        });

        setModalSuccess(
          "User created successfully."
        );
      }

      // Refresh WITHOUT replacing the page with loading screen
      await loadUsers(false);

      // Keep success message visible
      setTimeout(() => {
        closeUserModal();
      }, 1200);

    } catch (err) {
      console.error(
        "Failed to save user:",
        err
      );

      setModalError(
        err.response?.data?.message ||
          "Unable to save user. Please try again."
      );
    } finally {
      setSaving(false);
    }
  };

  // ---------------------------------------------------------
  // LOADING
  // ---------------------------------------------------------

  if (loading) {
    return (
      <div className="users-page">
        <div className="users-loading">
          Loading users...
        </div>
      </div>
    );
  }

  // ---------------------------------------------------------
  // UI
  // ---------------------------------------------------------

  return (
    <div className="users-page">

      {/* =====================================================
          HEADER
      ====================================================== */}

      <div className="users-header">

        <div>
          <span className="users-eyebrow">
            ADMINISTRATION
          </span>

          <h1>
            User Management
          </h1>

          <p>
            Manage users, roles and access across your workspace.
          </p>
        </div>

        <button
          className="create-user-button"
          onClick={openCreateModal}
        >
          <span>+</span>
          Create User
        </button>

      </div>

      {/* =====================================================
          PAGE SUCCESS MESSAGE
      ====================================================== */}

      {pageMessage && (
        <div className="users-page-message">
          <span>✓</span>
          {pageMessage}
        </div>
      )}

      {/* =====================================================
          STATS
      ====================================================== */}

      <div className="users-stats">

        <div className="user-stat-card">
          <div className="user-stat-icon">
            👥
          </div>

          <div>
            <span>Total Users</span>
            <strong>
              {users.length}
            </strong>
          </div>
        </div>

        <div className="user-stat-card">
          <div className="user-stat-icon">
            👑
          </div>

          <div>
            <span>Admins</span>
            <strong>
              {
                users.filter(
                  (user) =>
                    getRoleName(user.role) ===
                    "Admin"
                ).length
              }
            </strong>
          </div>
        </div>

        <div className="user-stat-card">
          <div className="user-stat-icon">
            💼
          </div>

          <div>
            <span>Managers</span>
            <strong>
              {
                users.filter(
                  (user) =>
                    getRoleName(user.role) ===
                    "Manager"
                ).length
              }
            </strong>
          </div>
        </div>

        <div className="user-stat-card">
          <div className="user-stat-icon">
            👤
          </div>

          <div>
            <span>Users</span>
            <strong>
              {
                users.filter(
                  (user) =>
                    getRoleName(user.role) ===
                    "User"
                ).length
              }
            </strong>
          </div>
        </div>

      </div>

      {/* =====================================================
          TOOLBAR
      ====================================================== */}

      <div className="users-toolbar">

        <div className="users-search">
          <span>⌕</span>

          <input
            type="text"
            placeholder="Search users by name or email..."
            value={search}
            onChange={(event) =>
              setSearch(event.target.value)
            }
          />
        </div>

        <select
          className="users-role-filter"
          value={roleFilter}
          onChange={(event) =>
            setRoleFilter(event.target.value)
          }
        >
          <option value="All">
            All Roles
          </option>

          <option value="Admin">
            Admin
          </option>

          <option value="Manager">
            Manager
          </option>

          <option value="User">
            User
          </option>
        </select>

        <button
          className="refresh-users-button"
          onClick={() => loadUsers(false)}
          title="Refresh users"
        >
          ↻
        </button>

      </div>

      {/* =====================================================
          USERS TABLE
      ====================================================== */}

      <div className="users-card">

        <div className="users-card-header">

          <div>
            <h2>
              All Users
            </h2>

            <p>
              Showing{" "}
              {filteredUsers.length === 0
                ? 0
                : (currentPage - 1) *
                    usersPerPage +
                  1}{" "}
              -
              {" "}
              {Math.min(
                currentPage * usersPerPage,
                filteredUsers.length
              )}{" "}
              of {filteredUsers.length} users
            </p>
          </div>

        </div>

        {paginatedUsers.length === 0 ? (

          <div className="users-empty">

            <div className="users-empty-icon">
              👥
            </div>

            <h3>
              No users found
            </h3>

            <p>
              Try changing your search or role filter.
            </p>

          </div>

        ) : (

          <div className="users-table-wrapper">

            <table className="users-table">

              <thead>
                <tr>
                  <th>User</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Created</th>
                  <th>Action</th>
                </tr>
              </thead>

              <tbody>

                {paginatedUsers.map((user) => {

                  const roleName =
                    getRoleName(user.role);

                  return (
                    <tr key={user.id}>

                      <td>

                        <div className="user-cell">

                          <div className="user-avatar">
                            {getInitials(user.name)}
                          </div>

                          <div className="user-name">

                            <strong>
                              {user.name}
                            </strong>

                            <span>
                              ID #{user.id}
                            </span>

                          </div>

                        </div>

                      </td>

                      <td>
                        <span className="user-email">
                          {user.email}
                        </span>
                      </td>

                      <td>
                        <span
                          className={`user-role ${getRoleClass(
                            user.role
                          )}`}
                        >
                          {roleName}
                        </span>
                      </td>

                      <td>

                        <span className="created-date">

                          {user.createdAt
                            ? new Date(
                                user.createdAt
                              ).toLocaleDateString(
                                "en-IN",
                                {
                                  day: "2-digit",
                                  month: "short",
                                  year: "numeric",
                                }
                              )
                            : "—"}

                        </span>

                      </td>

                      <td>

                        <button
                          className="user-action-button"
                          title={`Edit ${user.name}`}
                          onClick={() =>
                            openEditModal(user)
                          }
                        >
                          ✎
                        </button>

                      </td>

                    </tr>
                  );
                })}

              </tbody>

            </table>

          </div>

        )}

        {/* ===================================================
            PAGINATION
        ==================================================== */}

        {filteredUsers.length > usersPerPage && (

          <div className="users-pagination">

            <div className="pagination-info">
              Page {currentPage} of {totalPages}
            </div>

            <div className="pagination-controls">

              <button
                className="pagination-button"
                disabled={currentPage === 1}
                onClick={() =>
                  setCurrentPage(
                    (previous) =>
                      previous - 1
                  )
                }
              >
                ‹
              </button>

              {Array.from(
                { length: totalPages },
                (_, index) => index + 1
              )
                .filter((page) => {
                  if (totalPages <= 7) {
                    return true;
                  }

                  if (page === 1) {
                    return true;
                  }

                  if (page === totalPages) {
                    return true;
                  }

                  return (
                    page >= currentPage - 1 &&
                    page <= currentPage + 1
                  );
                })
                .map((page, index, pages) => {

                  const previousPage =
                    pages[index - 1];

                  const showDots =
                    previousPage &&
                    page - previousPage > 1;

                  return (
                    <span
                      key={page}
                      className="pagination-group"
                    >

                      {showDots && (
                        <span className="pagination-dots">
                          ...
                        </span>
                      )}

                      <button
                        className={`pagination-button ${
                          currentPage === page
                            ? "active"
                            : ""
                        }`}
                        onClick={() =>
                          setCurrentPage(page)
                        }
                      >
                        {page}
                      </button>

                    </span>
                  );
                })}

              <button
                className="pagination-button"
                disabled={
                  currentPage === totalPages
                }
                onClick={() =>
                  setCurrentPage(
                    (previous) =>
                      previous + 1
                  )
                }
              >
                ›
              </button>

            </div>

          </div>

        )}

      </div>

      {/* =====================================================
          CREATE / EDIT MODAL
      ====================================================== */}

      {showUserModal && (

        <div
          className="user-modal-overlay"
          onMouseDown={(event) => {
            if (
              event.target ===
              event.currentTarget
            ) {
              closeUserModal();
            }
          }}
        >

          <div className="user-modal">

            {/* MODAL HEADER */}

            <div className="user-modal-header">

              <div className="modal-title-area">

                <div className="modal-user-icon">
                  {editingUser ? "✎" : "+"}
                </div>

                <div>

                  <span className="user-modal-eyebrow">
                    {editingUser
                      ? "USER MANAGEMENT"
                      : "ADMINISTRATION"}
                  </span>

                  <h2>
                    {editingUser
                      ? "Edit User"
                      : "Create User"}
                  </h2>

                  <p>
                    {editingUser
                      ? "Update user details and role."
                      : "Add a new member to your workspace."}
                  </p>

                </div>

              </div>

              <button
                type="button"
                className="user-modal-close"
                onClick={closeUserModal}
                disabled={saving}
              >
                ×
              </button>

            </div>

            {/* FORM */}

            <form
              className="create-user-form"
              onSubmit={handleSubmit}
              autoComplete="off"
            >

              <div className="form-row">

                <div className="form-group">

                  <label htmlFor="user-name">
                    Full Name
                  </label>

                  <div className="input-wrapper">

                    <span className="input-icon">
                      👤
                    </span>

                    <input
                      id="user-name"
                      name="name"
                      type="text"
                      placeholder="Enter full name"
                      value={form.name}
                      onChange={handleFormChange}
                      disabled={saving}
                      autoComplete="off"
                      autoFocus
                    />

                  </div>

                </div>

                <div className="form-group">

                  <label htmlFor="user-role">
                    Role
                  </label>

                  <div className="input-wrapper">

                    <span className="input-icon">
                      ◉
                    </span>

                    <select
                      id="user-role"
                      name="role"
                      value={form.role}
                      onChange={handleFormChange}
                      disabled={saving}
                    >

                      <option value="2">
                        User
                      </option>

                      <option value="1">
                        Manager
                      </option>

                      <option value="0">
                        Admin
                      </option>

                    </select>

                  </div>

                </div>

              </div>

              <div className="form-group">

                <label htmlFor="user-email">
                  Email Address
                </label>

                <div className="input-wrapper">

                  <span className="input-icon">
                    @
                  </span>

                  <input
                    id="user-email"
                    name="email"
                    type="email"
                    placeholder="Enter email address"
                    value={form.email}
                    onChange={handleFormChange}
                    disabled={saving}
                    autoComplete="off"
                  />

                </div>

              </div>

              {!editingUser && (

                <div className="form-group">

                  <label htmlFor="user-password">
                    Password
                  </label>

                  <div className="input-wrapper">

                    <span className="input-icon">
                      •••
                    </span>

                    <input
                      id="user-password"
                      name="password"
                      type="password"
                      placeholder="Minimum 6 characters"
                      value={form.password}
                      onChange={handleFormChange}
                      disabled={saving}
                      autoComplete="new-password"
                    />

                  </div>

                  <small className="field-hint">
                    Password must contain at least 6 characters.
                  </small>

                </div>

              )}

              {/* ERROR */}

              {modalError && (

                <div className="create-user-message error">
                  <span>!</span>
                  {modalError}
                </div>

              )}

              {/* SUCCESS */}

              {modalSuccess && (

                <div className="create-user-message success">
                  <span>✓</span>
                  {modalSuccess}
                </div>

              )}

              {/* ACTIONS */}

              <div className="create-user-actions">

                <button
                  type="button"
                  className="create-user-cancel"
                  onClick={closeUserModal}
                  disabled={saving}
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  className="create-user-submit"
                  disabled={saving}
                >
                  {saving ? (
                    <>
                      <span className="button-spinner"></span>
                      Saving...
                    </>
                  ) : (
                    <>
                      {editingUser
                        ? "Save Changes"
                        : "Create User"}
                    </>
                  )}
                </button>

              </div>

            </form>

          </div>

        </div>

      )}

    </div>
  );
}

export default Users;