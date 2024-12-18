library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
--use IEEE.NUMERIC_STD.ALL;

library UNISIM;
use UNISIM.VComponents.all;

entity clk_div is
	generic ( n : natural := 50000);
    Port ( clk_in : in STD_LOGIC;
           clk_out : out STD_LOGIC);
end clk_div;

architecture Behavioral of clk_div is

	signal s_count : natural range 0 to n;
	signal s_slow_clk_en : std_logic;

begin

	process(clk_in)
	begin
		if rising_edge(clk_in) then
			if s_count < n-1 then
				s_count <= s_count + 1;
				s_slow_clk_en <= '0';
			else
				s_count <= 0;
				s_slow_clk_en <= '1';
			end if;				
		end if;
	end process;

	BUFGCE_inst : BUFGCE
	   port map (
		  O => clk_out,   -- 1-bit output: Clock output
		  CE => s_slow_clk_en, -- 1-bit input: Clock enable input for I0
		  I => clk_in    -- 1-bit input: Primary clock
	   );


end Behavioral;






-- library IEEE;
-- use IEEE.STD_LOGIC_1164.ALL;

-- entity clk_div is
--     generic(
--         n: integer range 2 to integer'HIGH := 10**6
--     );
--     Port (
--         clk_in: in std_logic;
--         clk_out: out std_logic
--     );
-- end clk_div;

-- architecture Behavioral of clk_div is
--     constant t_on: integer := n/2;
--     constant t_off: integer := (n-n/2);
--     signal cntr: integer range 0 to n;
-- begin
--     process(clk_in)
--     begin
--         if rising_edge(clk_in) then
--             if cntr < n-1 then
--                 cntr <= cntr + 1;
--             else
--                 cntr <=0;
--             end if;

--             if cntr < t_on then
--                 clk_out <= '1';
--             else
--                 clk_out <= '0';
--             end if;
--         end if;
--     end process;

-- end Behavioral;
