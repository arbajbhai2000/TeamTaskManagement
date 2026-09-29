import api from "./api";

const getTeams = async () => {
  const response = await api.get("/Teams");
  return response.data;
};

const getTeamById = async (teamId) => {
  const response = await api.get(`/Teams/${teamId}`);
  return response.data;
};

const createTeam = async (name, description) => {
  const response = await api.post("/Teams", {
    name,
    description,
  });

  return response.data;
};

const updateTeam = async (teamId, name, description) => {
  const response = await api.put(`/Teams/${teamId}`, {
    name,
    description,
  });

  return response.data;
};

const deleteTeam = async (teamId) => {
  await api.delete(`/Teams/${teamId}`);
};

const getTeamMembers = async (teamId) => {
  const response = await api.get(
    `/teams/${teamId}/members`
  );

  return response.data;
};

const addTeamMember = async (teamId, userId) => {
  const response = await api.post(
    `/teams/${teamId}/members`,
    {
      userId,
    }
  );

  return response.data;
};

const removeTeamMember = async (
  teamId,
  teamMemberId
) => {
  await api.delete(
    `/teams/${teamId}/members/${teamMemberId}`
  );
};

export default {
  getTeams,
  getTeamById,
  createTeam,
  updateTeam,
  deleteTeam,
  getTeamMembers,
  addTeamMember,
  removeTeamMember,
};