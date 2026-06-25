import { useEffect, useState } from 'react';
import { api, type LeaderboardEntry } from '../api';

export default function Leaderboard() {
  const [entries, setEntries] = useState<LeaderboardEntry[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.getLeaderboard().then(setEntries).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="container"><p className="empty">Loading...</p></div>;

  return (
    <div className="container">
      <div className="page-header"><h1>Leaderboard</h1></div>
      {entries.length === 0 ? <p className="empty">No scores yet.</p> : (
        <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
          <table>
            <thead>
              <tr><th>#</th><th>Player</th><th>Points</th><th>Level</th><th>Streak</th></tr>
            </thead>
            <tbody>
              {entries.map((e, i) => (
                <tr key={e.id} className={i === 0 ? 'rank-1' : i === 1 ? 'rank-2' : i === 2 ? 'rank-3' : ''}>
                  <td>{i + 1}</td>
                  <td>{e.username}</td>
                  <td>{e.totalPoints}</td>
                  <td><span className="badge badge-purple">Lv {e.level}</span></td>
                  <td>{e.currentStreak} 🔥</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
