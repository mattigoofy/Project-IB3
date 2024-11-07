library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use IEEE.NUMERIC_STD.ALL;

entity PWM_TB is
    generic(
        T_slow: time := 160ns;
        T_fast: time := 10ns;

        t_size_din: integer := 5;
        t_clk_times_faster: integer := 32;
        t_clk_times_faster_log2: integer := 5
    );
--  Port ( );
end PWM_TB;

architecture Behavioral of PWM_TB is
    component PWM is
        generic (
            size_din: integer := t_size_din;
            clk_times_faster: integer := t_clk_times_faster;
            clk_times_faster_log2: integer := t_clk_times_faster_log2
        );
        Port (
            din: std_logic_vector (size_din-1 downto 0);
            clk: in std_logic;
            clk_fast: in std_logic;
            dout: out std_logic
        );
    end component;

    signal t_din: std_logic_vector (t_size_din-1 downto 0) := (others =>'0');
    signal t_clk, t_clk_fast, t_dout: std_logic;
    signal t_cntr: integer range 0 to t_size_din-1 := 0;
begin
    inst_pwm: PWM
        port map(
            din => t_din,
            clk => t_clk,
            clk_fast => t_clk_fast,
            dout => t_dout
        );


    -- t_din <= x"95";

    process
    begin
        t_clk <= '1';
        wait for T_slow;
        t_clk <= '0';
        wait for T_slow;
    end process;

    process
    begin
        t_clk_fast <= '1';
        wait for T_fast;
        t_clk_fast <= '0';
        wait for T_fast;
    end process;

    process(t_clk)
    begin
        if(rising_edge(t_clk)) then
            if(t_cntr < t_size_din-1) then
                t_cntr <= t_cntr + 1;
            else
                t_cntr <= 0;
                if(to_integer(unsigned(t_din)) < 2**t_size_din-1) then
                    t_din <= std_logic_vector(to_unsigned(to_integer(unsigned(t_din)) + 1, t_size_din));
                else
                    t_din <= (others => '0');
                end if;
            end if;
        end if;
    end process;

end Behavioral;
