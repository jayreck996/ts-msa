import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { vi } from 'vitest';
import Quizzes from '../Quizzes';
import { api } from '../../api';

vi.mock('../../api', () => ({
  api: {
    getQuizzes: vi.fn(),
  },
}));

// Quizzes renders <Link> cards, which require a Router context.
const renderQuizzes = () => render(<MemoryRouter><Quizzes /></MemoryRouter>);

describe('Quizzes', () => {
  it('shows loading initially', () => {
    vi.mocked(api.getQuizzes).mockReturnValue(new Promise(() => {}));
    renderQuizzes();
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('renders quiz list', async () => {
    vi.mocked(api.getQuizzes).mockResolvedValue([
      { id: 1, title: 'C# Basics', description: '', difficulty: 'Easy', categoryId: 1, category: { id: 1, name: 'Tech' } },
    ]);
    renderQuizzes();
    await waitFor(() => expect(screen.getByText(/C# Basics/i)).toBeInTheDocument());
    expect(screen.getByText(/Easy/i)).toBeInTheDocument();
  });

  it('shows empty message when no quizzes', async () => {
    vi.mocked(api.getQuizzes).mockResolvedValue([]);
    renderQuizzes();
    await waitFor(() => expect(screen.getByText(/no quizzes yet/i)).toBeInTheDocument());
  });

  it('shows error when fetch fails', async () => {
    vi.mocked(api.getQuizzes).mockRejectedValue(new Error('Network error'));
    renderQuizzes();
    await waitFor(() => expect(screen.getByText(/failed to load/i)).toBeInTheDocument());
  });
});
