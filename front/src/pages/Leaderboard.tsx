import { useEffect, useState } from 'react';
import { api, type LeaderboardEntry } from '../api';

export default function Leaderboard() {
  const [entries, setEntries] = useState<LeaderboardEntry[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.getLeaderboard().then(setEntries).finally(() => setLoading(false));
  }, []);

  if (loading) return <p>Loading...</p>;

  return (
    <div>
      <h1>Leaderboard</h1>
      {entries.length === 0 && <p>No scores yet.</p>}
      <table>
        <thead>
          <tr><th>#</th><th>Player</th><th>Points</th><th>Level</th><th>Streak</th></tr>
        </thead>
        <tbody>
          {entries.map((e, i) => (
            <tr key={e.id}>
              <td>{i + 1}</td>
              <td>{e.username}</td>
              <td>{e.totalPoints}</td>
              <td>{e.level}</td>
              <td>{e.currentStreak}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
