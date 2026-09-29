import { useEffect, useState } from "react";
import userService from "../services/userService";
import "./Settings.css";

function Settings() {
  const [profile, setProfile] = useState({
    name: "",
    email: "",
    role: "",
  });

  const [name, setName] = useState("");

  const [passwordData, setPasswordData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });

  const [loading, setLoading] = useState(true);
  const [savingProfile, setSavingProfile] = useState(false);
  const [changingPassword, setChangingPassword] = useState(false);

  const [profileMessage, setProfileMessage] = useState("");
  const [passwordMessage, setPasswordMessage] = useState("");

  const [profileError, setProfileError] = useState("");
  const [passwordError, setPasswordError] = useState("");

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);

      const data = await userService.getMyProfile();

      setProfile(data);
      setName(data.name || "");
    } catch (error) {
      setProfileError(
        error.response?.data?.message ||
          "Failed to load profile."
      );
    } finally {
      setLoading(false);
    }
  };

  const handleProfileSubmit = async (event) => {
    event.preventDefault();

    setProfileMessage("");
    setProfileError("");

    if (!name.trim()) {
      setProfileError("Name is required.");
      return;
    }

    try {
      setSavingProfile(true);

      const updatedUser =
        await userService.updateMyProfile(
          name.trim()
        );

      setProfile(updatedUser);
      setName(updatedUser.name || "");

      setProfileMessage(
        "Profile updated successfully."
      );
    } catch (error) {
      setProfileError(
        error.response?.data?.message ||
          "Failed to update profile."
      );
    } finally {
      setSavingProfile(false);
    }
  };

  const handlePasswordChange = (
    event
  ) => {
    const { name, value } = event.target;

    setPasswordData((previous) => ({
      ...previous,
      [name]: value,
    }));
  };

  const handlePasswordSubmit = async (
    event
  ) => {
    event.preventDefault();

    setPasswordMessage("");
    setPasswordError("");

    if (
      !passwordData.currentPassword ||
      !passwordData.newPassword ||
      !passwordData.confirmPassword
    ) {
      setPasswordError(
        "Please fill in all password fields."
      );
      return;
    }

    if (
      passwordData.newPassword.length < 6
    ) {
      setPasswordError(
        "New password must be at least 6 characters."
      );
      return;
    }

    if (
      passwordData.newPassword !==
      passwordData.confirmPassword
    ) {
      setPasswordError(
        "New password and confirm password do not match."
      );
      return;
    }

    try {
      setChangingPassword(true);

      const response =
        await userService.changePassword(
          passwordData.currentPassword,
          passwordData.newPassword,
          passwordData.confirmPassword
        );

      setPasswordMessage(
        response.message ||
          "Password changed successfully."
      );

      setPasswordData({
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
      });
    } catch (error) {
      setPasswordError(
        error.response?.data?.message ||
          "Failed to change password."
      );
    } finally {
      setChangingPassword(false);
    }
  };

  if (loading) {
    return (
      <div className="settings-page">
        <div className="settings-loading">
          Loading settings...
        </div>
      </div>
    );
  }

  return (
    <div className="settings-page">
      <div className="settings-header">
        <div>
          <h1>Settings</h1>
          <p>
            Manage your profile and account security.
          </p>
        </div>
      </div>

      <div className="settings-grid">
        {/* Profile */}
        <section className="settings-card">
          <div className="settings-card-header">
            <div className="settings-icon profile-icon">
              👤
            </div>

            <div>
              <h2>Profile Information</h2>
              <p>
                Update your personal information.
              </p>
            </div>
          </div>

          <form
            onSubmit={handleProfileSubmit}
            className="settings-form"
          >
            <div className="profile-preview">
              <div className="profile-avatar">
                {profile.name
                  ? profile.name
                      .charAt(0)
                      .toUpperCase()
                  : "U"}
              </div>

              <div>
                <strong>
                  {profile.name || "User"}
                </strong>

                <span>
                  {profile.role || "User"}
                </span>
              </div>
            </div>

            <div className="form-group">
              <label>Name</label>

              <input
                type="text"
                value={name}
                onChange={(event) =>
                  setName(event.target.value)
                }
                placeholder="Enter your name"
              />
            </div>

            <div className="form-group">
              <label>Email</label>

              <input
                type="email"
                value={profile.email}
                disabled
              />

              <small>
                Email address cannot be changed.
              </small>
            </div>

            {profileError && (
              <div className="settings-error">
                {profileError}
              </div>
            )}

            {profileMessage && (
              <div className="settings-success">
                {profileMessage}
              </div>
            )}

            <button
              type="submit"
              className="settings-button"
              disabled={savingProfile}
            >
              {savingProfile
                ? "Saving..."
                : "Save Changes"}
            </button>
          </form>
        </section>

        {/* Password */}
        <section className="settings-card">
          <div className="settings-card-header">
            <div className="settings-icon security-icon">
              🔒
            </div>

            <div>
              <h2>Change Password</h2>
              <p>
                Keep your account secure with a strong password.
              </p>
            </div>
          </div>

          <form
            onSubmit={handlePasswordSubmit}
            className="settings-form"
          >
            <div className="form-group">
              <label>Current Password</label>

              <input
                type="password"
                name="currentPassword"
                value={
                  passwordData.currentPassword
                }
                onChange={
                  handlePasswordChange
                }
                placeholder="Enter current password"
              />
            </div>

            <div className="form-group">
              <label>New Password</label>

              <input
                type="password"
                name="newPassword"
                value={
                  passwordData.newPassword
                }
                onChange={
                  handlePasswordChange
                }
                placeholder="Enter new password"
              />

              <small>
                Minimum 6 characters.
              </small>
            </div>

            <div className="form-group">
              <label>Confirm New Password</label>

              <input
                type="password"
                name="confirmPassword"
                value={
                  passwordData.confirmPassword
                }
                onChange={
                  handlePasswordChange
                }
                placeholder="Confirm new password"
              />
            </div>

            {passwordError && (
              <div className="settings-error">
                {passwordError}
              </div>
            )}

            {passwordMessage && (
              <div className="settings-success">
                {passwordMessage}
              </div>
            )}

            <button
              type="submit"
              className="settings-button"
              disabled={changingPassword}
            >
              {changingPassword
                ? "Updating..."
                : "Change Password"}
            </button>
          </form>
        </section>
      </div>
    </div>
  );
}

export default Settings;