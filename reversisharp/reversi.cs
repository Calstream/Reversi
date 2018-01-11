using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace reversisharp
{
    public struct possible_move
    {
        public bool color; // 0 - black, 1 - white
        public int pos;
        public List<int> flipped; // flipped discs
    }

    class reversi
    {

        int size = 8;
        public int[] state = new int[64];
        public List<int> white;
        public List<int> black;

        public reversi parent = null;

        int[] weights = new int[64] // disc values
        { 30, 4, 2, 2, 2, 2, 4, 30 , 
         4, -10, 1, 1, 1, 1, -10, 4 , 
         2, 1, 6, 6, 6, 6, 1, 2 , 
         2, 1, 6, 6, 6, 6, 1, 2 , 
         2, 1, 6, 6, 6, 6, 1, 2 , 
         2, 1, 6, 6, 6, 6, 1, 2 , 
         4, -10, 1, 1, 1, 1, -10, 4 , 
         30, 4, 2, 2, 2, 2, 4, 30  }; 

        public reversi(List<int> b, List<int> w)
        {
            for (int i = 0; i < size * size; i++)
                state[i] = 2;
			black = new List<int>(b.Count);
			white = new List<int>(w.Count);
			foreach (int x in b)
			{
				state[x] = 0;
				black.Add(x);
			}
			foreach (int x in w)
			{
				state[x] = 1;
				white.Add(x);
			}
		}

        public reversi()
        {
            white = new List<int>();
            black = new List<int>();
            for (int i = 0; i < size * size; i++)
            {
                if (i == 27 || i == 36) //white
                {
                    white.Add(i);
                    state[i] = 1;
                }

                else if (i == 28 || i == 35) //black
                {
                    black.Add(i);
                    state[i] = 0;
                }
                else state[i] = 2; // empty
            }

        }

        public bool game_over()
        {
            if (white.Count + black.Count == 64) //board full
                return true;
            else if (this.get_frontier(false) == null &&
                this.get_frontier(true) == null) // no moves
                return true;
            return false;
        }

       

        class pm_comparer : IComparer<possible_move>
        {
            public int Compare(possible_move pm1, possible_move pm2)
            {
                return pm1.pos.CompareTo(pm2.pos);
            }
        }

        int get_x(int pos)
        {
            return pos / size;
        }

        int get_y(int pos)
        {
            return pos % size;
        }

        

        public List<possible_move> get_frontier(bool player) //player - 0/1
        {
            List<possible_move> result = new List<possible_move>();
            List<int> curr_cells = black;
            if (player)
                curr_cells = white;

            foreach (int pos in curr_cells)
            {

                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        int x = get_x(pos);
                        int y = get_y(pos);
                        List<int> flipped = new List<int>();
                        while (x >= 0 && x <= 7 && y >= 0 && y <= 7)
                        {
                            x += dx;
                            y += dy;

                            if (x < 0 || x > 7 || y < 0 || y > 7)
                                continue;
                            
                            int new_pos = x * 8 + y;
                            int p = 0;
                            if (player)
                                p = 1;
                            if (state[new_pos] == p) // same color 
                                break;
                            if (state[new_pos] == 2) // empty cell
                            {
                                if (flipped.Count > 0)
                                {
                                    possible_move pos_m = new possible_move();
                                    pos_m.color = player;
                                    pos_m.flipped = flipped;
                                    pos_m.pos = new_pos;
                                    result.Add(pos_m);
                                }
                                break;
                            }
                            // enemy color
                            flipped.Add(new_pos);
                        }

                    }
            }
            if (result.Count == 0) // ходов нет
                return null;
            if (result.Count > 1)
            {
                result = result.OrderBy(p => p.pos).ToList();
                int current = 0;
                var pm = result[current];

                var next = result[current + 1];
                while (current != result.Count() - 1)
                {
                    if (current == result.Count() - 1)
                        break;

                    if (next.pos == pm.pos)
                    {
                        pm.flipped.AddRange(next.flipped);
                        result.Remove(next);
                        if (current < result.Count() - 1)
                        {
                            next = result[current + 1];
                        }
                        else break;
                    }
                    else
                    {
                        pm = next;
                        ++current;

                        if (current < result.Count() - 1)
                        {
                            next = result[current + 1];
                        }
                    }
                }
            }
			return result;
        }

        int eval_state(bool player)
        {
            int result = 0;
            if (get_frontier(!player) == null)
                if (player)
                    result -= 35;
                else result += 25;
            List<int> curr_cells = black;
            if (player)
                curr_cells = white;
            foreach (int pos in curr_cells)
				if(player)
                result -= weights[pos];
			else result += weights[pos];
			return result;

        }

        int alphabeta(reversi st, int depth, int α, int β, bool maximizing_player)
        {
            
            var frontier = st.get_frontier(maximizing_player);
            if (depth == 0 || frontier == null)
                return st.eval_state(maximizing_player);
            possible_move bm;
            if (!maximizing_player)
            {
                int v = -100000;
                foreach (var pos in frontier)
                {
                    bm = pos;
                    reversi child = st.make_move(pos);
                    v = Math.Max(v, st.alphabeta(child, depth - 1, α, β, !maximizing_player));
                    α = Math.Max(α, v);
                    if (β <= α)
                        break;// (*β cut - off *)
                }
                return v;
            }
            else
            {
                int v = 100000;
                foreach (var pos in frontier)
                {
                    bm = pos;
                    reversi child = st.make_move(pos);
                    v = Math.Min(v, st.alphabeta(child, depth - 1, α, β, !maximizing_player));
                    β = Math.Min(β, v);
                    if (β <= α)
                        break;// (*β cut - off *)
                }
                return v;
            }
        }
        public possible_move ai_make_move(int limit)
        {
            var frontier = get_frontier(false);
            int[] h = new int[frontier.Count];
            int i = 0;
            foreach (var m in frontier)
            {
                reversi child = make_move(m);
                h[i] = alphabeta(child, limit, -10000, 10000, false);
                ++i;
            }
			possible_move result = frontier[0];
			int g = h[0];
			for (int j = 1; j < frontier.Count; ++j)
			{
                if (h[j] > g)
                {
                    result = frontier[j];
                    g = h[j];
                }
                else if (h[j] == g)
                {
                    Random random = new Random();
                    if (random.NextDouble() > 0.5)
                        g = h[j];
                }
            }
			return result;
        }

        public reversi make_move(possible_move move)
        {
			reversi result = new reversi(this.black, this.white);
            result.parent = this;
            var self = result.black;
            var enemy = result.white;
            if (move.color) //white
            {
                self = result.white;
                enemy = result.black;
            }
            int st = 0;
            if (move.color)
                st = 1;
            result.state[move.pos] = st;
            self.Add(move.pos);
            foreach (int x in move.flipped)
            {
                result.state[x] = st;
                self.Add(x);
                enemy.RemoveAt(enemy.FindIndex(a => a ==x ));
            }
            return result;
        }

    };
}
