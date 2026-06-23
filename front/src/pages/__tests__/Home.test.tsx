import { render, screen } from '@testing-library/react';
import Home from '../Home';

describe('Home', () => {
  it('renders the heading', () => {
    render(<Home />);
    expect(screen.getByRole('heading', { name: /QuizQuest/i })).toBeInTheDocument();
  });

  it('renders the tagline', () => {
    render(<Home />);
    expect(screen.getByText(/earn points/i)).toBeInTheDocument();
  });
});
