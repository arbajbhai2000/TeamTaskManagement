import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../services/api";
import teamService from "../services/teamService";
import "./CreateTask.css";

function CreateTask() {
  const navigate = useNavigate();

  const [teams, setTeams] = useState([]);
  const [members, setMembers] = useState([]);

  const [loadingTeams, setLoadingTeams] = useState(true);
  const [loadingMembers, setLoadingMembers] = useState(false);
  const [saving, setSaving] = useState(false);

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [form, setForm] = useState({
    title: "",
    description: "",
    teamId: "",
    assignedToId: "",
    priority: "2",
    dueDate: "",
  });

  useEffect(() => {
    const token = localStorage.getItem("accessToken");

    if (!token) {
      navigate("/login");
      return;
    }

    loadTeams();
  }, [navigate]);

  const loadTeams = async () => {
    try {
      setLoadingTeams(true);
      setError("");

      const data = await teamService.getTeams();

      setTeams(data || []);
    } catch (err) {
      console.error("Failed to load teams:", err);

      setError(
        err.response?.data?.message ||
          "Unable to load teams. Please try again."
      );
    } finally {
      setLoadingTeams(false);
    }
  };

  const loadTeamMembers = async (teamId) => {
    if (!teamId) {
      setMembers([]);
      return;
    }

    try {
      setLoadingMembers(true);
      setError("");

      const data =
        await teamService.getTeamMembers(
          Number(teamId)
        );

      setMembers(data || []);
    } catch (err) {
      console.error(
        "Failed to load team members:",
        err
      );

      setMembers([]);

      setError(
        err.response?.data?.message ||
          "Unable to load team members."
      );
    } finally {
      setLoadingMembers(false);
    }
  };

  const handleChange = (event) => {
    const { name, value } = event.target;

    setForm((previous) => ({
      ...previous,
      [name]: value,
    }));

    setError("");
    setSuccess("");
  };

  const handleTeamChange = async (event) => {
    const teamId = event.target.value;

    setForm((previous) => ({
      ...previous,
      teamId,
      assignedToId: "",
    }));

    setMembers([]);
    setError("");
    setSuccess("");

    if (teamId) {
      await loadTeamMembers(teamId);
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    setError("");
    setSuccess("");

    if (!form.title.trim()) {
      setError("Task title is required.");
      return;
    }

    if (!form.teamId) {
      setError("Please select a team.");
      return;
    }

    if (!form.assignedToId) {
      setError("Please select a team member.");
      return;
    }

    try {
      setSaving(true);

      const request = {
        title: form.title.trim(),
        description: form.description.trim(),
        priority: Number(form.priority),
        dueDate: form.dueDate
          ? new Date(
              `${form.dueDate}T23:59:59`
            ).toISOString()
          : null,
        assignedToId: Number(
          form.assignedToId
        ),
        teamId: Number(form.teamId),
      };

      await api.post("/Tasks", request);

      setSuccess(
        "Task created successfully."
      );

      setTimeout(() => {
        navigate("/tasks");
      }, 1000);
    } catch (err) {
      console.error(
        "Failed to create task:",
        err
      );

      setError(
        err.response?.data?.message ||
          "Unable to create task. Please try again."
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="create-task-page">
      <div className="create-task-container">

        {/* Header */}
        <div className="create-task-header">
          <div>
            <button
              type="button"
              className="create-task-back-button"
              onClick={() => navigate("/tasks")}
            >
              ← Back
            </button>

            <span className="create-task-eyebrow">
              TASK MANAGEMENT
            </span>

            <h1>Create Task</h1>

            <p>
              Create a new task and assign it
              to a team member.
            </p>
          </div>
        </div>

        {/* Form Card */}
        <div className="create-task-card">

          <form onSubmit={handleSubmit}>

            {/* Task Title */}
            <div className="create-task-form-group">
              <label htmlFor="title">
                Task Title
              </label>

              <input
                id="title"
                name="title"
                type="text"
                value={form.title}
                onChange={handleChange}
                placeholder="Enter task title"
                maxLength={150}
                disabled={saving}
              />
            </div>

            {/* Description */}
            <div className="create-task-form-group">
              <label htmlFor="description">
                Description
              </label>

              <textarea
                id="description"
                name="description"
                value={form.description}
                onChange={handleChange}
                placeholder="Describe the task..."
                rows={5}
                maxLength={1000}
                disabled={saving}
              />

              <small>
                {form.description.length}/1000
              </small>
            </div>

            {/* Team + Assign To */}
            <div className="create-task-form-row">

              {/* Team */}
              <div className="create-task-form-group">
                <label htmlFor="teamId">
                  Team
                </label>

                <select
                  id="teamId"
                  name="teamId"
                  value={form.teamId}
                  onChange={handleTeamChange}
                  disabled={
                    saving || loadingTeams
                  }
                >
                  <option value="">
                    {loadingTeams
                      ? "Loading teams..."
                      : "Select a team"}
                  </option>

                  {teams.map((team) => (
                    <option
                      key={team.id}
                      value={team.id}
                    >
                      {team.name}
                    </option>
                  ))}
                </select>
              </div>

              {/* Assign To */}
              <div className="create-task-form-group">
                <label htmlFor="assignedToId">
                  Assign To
                </label>

                <select
                  id="assignedToId"
                  name="assignedToId"
                  value={form.assignedToId}
                  onChange={handleChange}
                  disabled={
                    saving ||
                    !form.teamId ||
                    loadingMembers
                  }
                >
                  <option value="">
                    {!form.teamId
                      ? "Select a team first"
                      : loadingMembers
                      ? "Loading members..."
                      : members.length === 0
                      ? "No members available"
                      : "Select a member"}
                  </option>

                  {members.map((member) => (
                    <option
                      key={member.userId}
                      value={member.userId}
                    >
                      {member.userName}
                      {member.userEmail
                        ? ` — ${member.userEmail}`
                        : ""}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {/* Priority + Due Date */}
            <div className="create-task-form-row">

              {/* Priority */}
              <div className="create-task-form-group">
                <label htmlFor="priority">
                  Priority
                </label>

                <select
                  id="priority"
                  name="priority"
                  value={form.priority}
                  onChange={handleChange}
                  disabled={saving}
                >
                  <option value="1">
                    Low
                  </option>

                  <option value="2">
                    Medium
                  </option>

                  <option value="3">
                    High
                  </option>
                </select>
              </div>

              {/* Due Date */}
              <div className="create-task-form-group">
                <label htmlFor="dueDate">
                  Due Date
                </label>

                <input
                  id="dueDate"
                  name="dueDate"
                  type="date"
                  value={form.dueDate}
                  onChange={handleChange}
                  disabled={saving}
                />
              </div>
            </div>

            {/* Messages */}
            {error && (
              <div className="create-task-error">
                {error}
              </div>
            )}

            {success && (
              <div className="create-task-success">
                {success}
              </div>
            )}

            {/* Actions */}
            <div className="create-task-actions">

              <button
                type="button"
                className="create-task-cancel-button"
                onClick={() => navigate("/tasks")}
                disabled={saving}
              >
                Cancel
              </button>

              <button
                type="submit"
                className="create-task-submit-button"
                disabled={saving}
              >
                {saving
                  ? "Creating..."
                  : "Create Task"}
              </button>

            </div>

          </form>

        </div>
      </div>
    </div>
  );
}

export default CreateTask;