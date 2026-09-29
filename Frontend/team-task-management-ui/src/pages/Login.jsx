import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import api from "../services/api";
import "./Login.css";

function Login() {
const location = useLocation();
const navigate = useNavigate();

const [email, setEmail] = useState(location.state?.email || "");
const [password, setPassword] = useState("");
const [showPassword, setShowPassword] = useState(false);
const [error, setError] = useState("");
const [loading, setLoading] = useState(false);

const handleLogin = async (event) => {
event.preventDefault();


setError("");
setLoading(true);

try {
  const response = await api.post("/Auth/login", {
    email,
    password,
  });

  const { accessToken, refreshToken, expiresAt } = response.data;

  localStorage.setItem("accessToken", accessToken);
  localStorage.setItem("refreshToken", refreshToken);
  localStorage.setItem("expiresAt", expiresAt);

  navigate("/dashboard");
} catch (err) {
  setError(
    err.response?.data?.message ||
      "Login failed. Please check your email and password."
  );
} finally {
  setLoading(false);
}


};

return ( <div className="login-page"> <div className="login-container"> <div className="login-info"> <div className="brand"> <div className="brand-icon">✓</div> <span>Team Task Management</span> </div>


      <div className="info-content">
        <h1>Manage your team. Complete your tasks.</h1>

        <p>
          A simple and powerful workspace for managing teams,
          tasks, collaboration, and progress.
        </p>

        <div className="features">
          <div>✓ Manage teams efficiently</div>
          <div>✓ Track tasks and progress</div>
          <div>✓ Collaborate with your team</div>
          <div>✓ Stay updated with notifications</div>
        </div>
      </div>
    </div>

    <div className="login-card">
      <div className="login-header">
        <h2>Welcome Back</h2>
        <p>Sign in to continue to your account</p>
      </div>

      <form onSubmit={handleLogin} autoComplete="off">
        <div className="form-group">
          <label htmlFor="email">Email Address</label>

        <input
          id="email"
          name="email"
          type="email"
          placeholder="Enter your email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          autoComplete="off"
          required
        />
      </div>

        <div className="form-group">
          <label htmlFor="password">Password</label>

        <div className="password-wrapper">
          <input
            id="password"
            name="password"
            type={showPassword ? "text" : "password"}
            placeholder="Enter your password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            autoComplete="new-password"
            required
          />

          <button
              type="button"
              className="password-toggle"
              onClick={() => setShowPassword(!showPassword)}
            >
              {showPassword ? "Hide" : "Show"}
            </button>
          </div>
        </div>

        {error && <div className="login-error">{error}</div>}

        <button
          type="submit"
          className="login-button"
          disabled={loading}
        >
          {loading ? "Signing in..." : "Sign In"}
        </button>
      </form>

      <div className="register-link">
        Don't have an account?{" "}
        <Link to="/register">Create an account</Link>
      </div>
    </div>
  </div>
</div>


);
}

export default Login;
