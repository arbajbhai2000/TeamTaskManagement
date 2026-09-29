import { useEffect, useState } from "react";
import teamService from "../services/teamService";
import api from "../services/api";
import "./Teams.css";

function Teams() {
  const [teams, setTeams] = useState([]);
  const [users, setUsers] = useState([]);
  const [selectedTeam, setSelectedTeam] = useState(null);
  const [members, setMembers] = useState([]);

  const [loading, setLoading] = useState(true);
  const [loadingMembers, setLoadingMembers] = useState(false);
  const [saving, setSaving] = useState(false);

  const [showTeamModal, setShowTeamModal] = useState(false);
  const [showMembersModal, setShowMembersModal] = useState(false);

  const [editingTeam, setEditingTeam] = useState(null);

  const [teamForm, setTeamForm] = useState({
    name: "",
    description: "",
  });

  const [selectedUserId, setSelectedUserId] = useState("");

  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  // --------------------------------------------------
  // ROLE
  // --------------------------------------------------

  const getUserRole = () => {
    try {
      const token = localStorage.getItem("accessToken");

      if (!token) return "";

      const payload = JSON.parse(atob(token.split(".")[1]));

      return (
        payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] ||
        payload.role ||
        ""
      );
    } catch (err) {
      console.error("Failed to read user role.", err);
      return "";
    }
  };

  const userRole = getUserRole();

  const isAdmin = userRole === "Admin";
  const isManager = userRole === "Manager";
  const canManageMembers = isAdmin || isManager;

  // --------------------------------------------------
  // LOAD DATA
  // --------------------------------------------------

  useEffect(() => {
    loadTeams();
    loadUsers();
  }, []);

  const loadTeams = async () => {
    try {
      setLoading(true);
      setError("");

      const data = await teamService.getTeams();

      setTeams(data);
    } catch (err) {
      setError(
        err.response?.data?.message ||
          "Failed to load teams."
      );
    } finally {
      setLoading(false);
    }
  };

  const loadUsers = async () => {
    try {
      const response = await api.get("/Users");

      setUsers(response.data);
    } catch (err) {
      console.error("Failed to load users.", err);
    }
  };

  // --------------------------------------------------
  // TEAM MODAL
  // --------------------------------------------------

  const openCreateModal = () => {
    if (!isAdmin) return;

    setEditingTeam(null);

    setTeamForm({
      name: "",
      description: "",
    });

    setMessage("");
    setError("");

    setShowTeamModal(true);
  };

  const openEditModal = (team) => {
    if (!isAdmin) return;

    setEditingTeam(team);

    setTeamForm({
      name: team.name || "",
      description: team.description || "",
    });

    setMessage("");
    setError("");

    setShowTeamModal(true);
  };

  const closeTeamModal = () => {
    if (saving) return;

    setShowTeamModal(false);
    setEditingTeam(null);
  };

  const handleTeamFormChange = (event) => {
    const { name, value } = event.target;

    setTeamForm((previous) => ({
      ...previous,
      [name]: value,
    }));
  };

  const handleTeamSubmit = async (event) => {
    event.preventDefault();

    if (!isAdmin) return;

    setMessage("");
    setError("");

    if (!teamForm.name.trim()) {
      setError("Team name is required.");
      return;
    }

    try {
      setSaving(true);

      if (editingTeam) {
        await teamService.updateTeam(
          editingTeam.id,
          teamForm.name.trim(),
          teamForm.description.trim()
        );

        setMessage("Team updated successfully.");
      } else {
        await teamService.createTeam(
          teamForm.name.trim(),
          teamForm.description.trim()
        );

        setMessage("Team created successfully.");
      }

      await loadTeams();

      setShowTeamModal(false);
      setEditingTeam(null);
    } catch (err) {
      setError(
        err.response?.data?.message ||
          "Failed to save team."
      );
    } finally {
      setSaving(false);
    }
  };

  // --------------------------------------------------
  // DELETE TEAM
  // --------------------------------------------------

  const handleDeleteTeam = async (team) => {
    if (!isAdmin) return;

    const confirmed = window.confirm(
      `Are you sure you want to delete "${team.name}"?`
    );

    if (!confirmed) return;

    try {
      setError("");
      setMessage("");

      await teamService.deleteTeam(team.id);

      setMessage("Team deleted successfully.");

      if (selectedTeam?.id === team.id) {
        setSelectedTeam(null);
        setMembers([]);
        setShowMembersModal(false);
      }

      await loadTeams();
    } catch (err) {
      setError(
        err.response?.data?.message ||
          "Failed to delete team."
      );
    }
  };

  // --------------------------------------------------
  // MEMBERS
  // --------------------------------------------------

  const openMembersModal = async (team) => {
    try {
      setSelectedTeam(team);
      setSelectedUserId("");
      setShowMembersModal(true);
      setLoadingMembers(true);

      setError("");
      setMessage("");

      const data =
        await teamService.getTeamMembers(team.id);

      setMembers(data);
    } catch (err) {
      setError(
        err.response?.data?.message ||
          "Failed to load team members."
      );
    } finally {
      setLoadingMembers(false);
    }
  };

  const closeMembersModal = () => {
    setShowMembersModal(false);
    setSelectedTeam(null);
    setMembers([]);
    setSelectedUserId("");
  };

  const handleAddMember = async () => {
    if (!canManageMembers) return;

    if (!selectedTeam || !selectedUserId) {
      setError("Please select a user.");
      return;
    }

    try {
      setError("");
      setMessage("");

      await teamService.addTeamMember(
        selectedTeam.id,
        Number(selectedUserId)
      );

      const updatedMembers =
        await teamService.getTeamMembers(
          selectedTeam.id
        );

      setMembers(updatedMembers);
      setSelectedUserId("");

      await loadTeams();

      setMessage("Member added successfully.");
      setTimeout(() => {
        setMessage("");
      }, 2000);
    } catch (err) {
      setError(
        err.response?.data?.message ||
          "Failed to add member."
      );
    }
  };

  const handleRemoveMember = async (member) => {
    if (!canManageMembers) return;

    const confirmed = window.confirm(
      `Remove ${member.userName} from this team?`
    );

    if (!confirmed) return;

    try {
      setError("");
      setMessage("");

      await teamService.removeTeamMember(
        selectedTeam.id,
        member.id
      );

      const updatedMembers =
        await teamService.getTeamMembers(
          selectedTeam.id
        );

      setMembers(updatedMembers);

      await loadTeams();

      setMessage("Member removed successfully.");
      setTimeout(() => {
        setMessage("");
      }, 2000);
    } catch (err) {
      setError(
        err.response?.data?.message ||
          "Failed to remove member."
      );
    }
  };

  const availableUsers = users.filter(
    (user) =>
      !members.some(
        (member) => member.userId === user.id
      )
  );

  // --------------------------------------------------
  // LOADING
  // --------------------------------------------------

  if (loading) {
    return (
      <div className="teams-page">
        <div className="teams-loading">
          Loading teams...
        </div>
      </div>
    );
  }

  // --------------------------------------------------
  // UI
  // --------------------------------------------------

  return (
    <div className="teams-page">

      {/* HEADER */}

      <div className="teams-header">

        <div>

          <span className="teams-eyebrow">
            WORKSPACE
          </span>

          <h1>Teams</h1>

          <p>
            Organize your workspace and manage team members.
          </p>

        </div>

        {/* ADMIN ONLY */}

        {isAdmin && (
          <button
            className="create-team-button"
            onClick={openCreateModal}
          >
            <span>+</span>
            Create Team
          </button>
        )}

      </div>

      {/* SUCCESS MESSAGE */}

      {message && (
        <div className="teams-success">
          {message}
        </div>
      )}

      {/* ERROR MESSAGE */}

      {error &&
        !showTeamModal &&
        !showMembersModal && (
          <div className="teams-error">
            {error}
          </div>
        )}

      {/* EMPTY */}

      {teams.length === 0 ? (

        <div className="teams-empty">

          <div className="teams-empty-icon">
            👥
          </div>

          <h2>No teams yet</h2>

          <p>
            {isAdmin
              ? "Create your first team to start organizing members and tasks."
              : "No teams are currently assigned to you."
            }
          </p>

          {/* ADMIN ONLY */}

          {isAdmin && (
            <button
              className="create-team-button"
              onClick={openCreateModal}
            >
              + Create Team
            </button>
          )}

        </div>

      ) : (

        <div className="teams-grid">

          {teams.map((team) => (

            <div
              className="team-card"
              key={team.id}
            >

              {/* CARD TOP */}

              <div className="team-card-top">

                <div className="team-avatar">

                  {team.name
                    ? team.name
                        .charAt(0)
                        .toUpperCase()
                    : "T"}

                </div>

                {/* ADMIN ONLY */}

                {isAdmin && (
                  <div className="team-actions">

                    <button
                      className="team-action edit"
                      onClick={() =>
                        openEditModal(team)
                      }
                      title="Edit team"
                    >
                      ✎
                    </button>

                    <button
                      className="team-action delete"
                      onClick={() =>
                        handleDeleteTeam(team)
                      }
                      title="Delete team"
                    >
                      🗑
                    </button>

                  </div>
                )}

              </div>

              <h2>{team.name}</h2>

              <p className="team-description">

                {team.description ||
                  "No description provided."}

              </p>

              {/* TEAM META */}

              <div className="team-meta">

                <div>

                  <span className="meta-label">
                    MEMBERS
                  </span>

                  <strong>
                    {team.memberCount}
                  </strong>

                </div>

                <div>

                  <span className="meta-label">
                    CREATED BY
                  </span>

                  <strong>
                    {team.createdByName ||
                      "Unknown"}
                  </strong>

                </div>

              </div>

              {/* VIEW MEMBERS */}

              <button
                className="view-members-button"
                onClick={() =>
                  openMembersModal(team)
                }
              >
                View Members
                <span>→</span>
              </button>

            </div>

          ))}

        </div>

      )}

      {/* CREATE / EDIT TEAM MODAL */}

      {showTeamModal && isAdmin && (

        <div className="teams-modal-overlay">

          <div className="teams-modal">

            <div className="modal-header">

              <div>

                <h2>
                  {editingTeam
                    ? "Edit Team"
                    : "Create Team"}
                </h2>

                <p>
                  {editingTeam
                    ? "Update team information."
                    : "Create a new workspace team."}
                </p>

              </div>

              <button
                className="modal-close"
                onClick={closeTeamModal}
                disabled={saving}
              >
                ×
              </button>

            </div>

            <form
              onSubmit={handleTeamSubmit}
              className="team-form"
            >

              <div className="form-group">

                <label>
                  Team Name
                </label>

                <input
                  type="text"
                  name="name"
                  value={teamForm.name}
                  onChange={
                    handleTeamFormChange
                  }
                  placeholder="Enter team name"
                  disabled={saving}
                />

              </div>

              <div className="form-group">

                <label>
                  Description
                </label>

                <textarea
                  name="description"
                  value={
                    teamForm.description
                  }
                  onChange={
                    handleTeamFormChange
                  }
                  placeholder="Enter team description"
                  rows="4"
                  disabled={saving}
                />

              </div>

              {error && (
                <div className="teams-error">
                  {error}
                </div>
              )}

              <div className="modal-footer">

                <button
                  type="button"
                  className="cancel-button"
                  onClick={closeTeamModal}
                  disabled={saving}
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  className="save-team-button"
                  disabled={saving}
                >
                  {saving
                    ? "Saving..."
                    : editingTeam
                    ? "Update Team"
                    : "Create Team"}
                </button>

              </div>

            </form>

          </div>

        </div>

      )}

      {/* MEMBERS MODAL */}

      {showMembersModal &&
        selectedTeam && (

          <div className="teams-modal-overlay">

            <div className="teams-modal members-modal">

              <div className="modal-header">

                <div>

                  <h2>
                    {selectedTeam.name}
                  </h2>

                  <p>
                    {canManageMembers
                      ? "Manage team members."
                      : "View team members."
                    }
                  </p>

                </div>

                <button
                  className="modal-close"
                  onClick={closeMembersModal}
                >
                  ×
                </button>

              </div>

              {/* ADMIN + MANAGER ONLY */}

              {canManageMembers && (
                <div className="add-member-section">

                  <label>
                    Add Team Member
                  </label>

                  <div className="add-member-row">

                    <select
                      value={selectedUserId}
                      onChange={(event) =>
                        setSelectedUserId(
                          event.target.value
                        )
                      }
                    >

                      <option value="">
                        Select a user
                      </option>

                      {availableUsers.map(
                        (user) => (
                          <option
                            key={user.id}
                            value={user.id}
                          >
                            {user.name} —{" "}
                            {user.email}
                          </option>
                        )
                      )}

                    </select>

                    <button
                      className="add-member-button"
                      onClick={handleAddMember}
                    >
                      Add
                    </button>

                  </div>

                </div>
              )}

              {error && (
                <div className="teams-error">
                  {error}
                </div>
              )}

              {message && (
                <div className="teams-success">
                  {message}
                </div>
              )}

              {/* MEMBERS LIST */}

              <div className="members-list">

                <div className="members-list-header">

                  <span>
                    Members ({members.length})
                  </span>

                </div>

                {loadingMembers ? (

                  <div className="members-loading">
                    Loading members...
                  </div>

                ) : members.length === 0 ? (

                  <div className="members-empty">
                    No members assigned to this team.
                  </div>

                ) : (

                  members.map((member) => (

                    <div
                      className="member-row"
                      key={member.id}
                    >

                      <div className="member-avatar">

                        {member.userName
                          ? member.userName
                              .charAt(0)
                              .toUpperCase()
                          : "U"}

                      </div>

                      <div className="member-info">

                        <strong>
                          {member.userName}
                        </strong>

                        <span>
                          {member.userEmail}
                        </span>

                      </div>

                      {/* ADMIN + MANAGER ONLY */}

                      {canManageMembers && (
                        <button
                          className="remove-member-button"
                          onClick={() =>
                            handleRemoveMember(
                              member
                            )
                          }
                        >
                          Remove
                        </button>
                      )}

                    </div>

                  ))

                )}

              </div>

            </div>

          </div>

        )}

    </div>
  );
}

export default Teams;