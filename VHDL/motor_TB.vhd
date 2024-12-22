library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use IEEE.NUMERIC_STD.ALL;

entity motor_TB is
    generic(
        T_slow: time := 160ns;
        T_fast: time := 10ns;
        din_length: integer := 4
    );
end motor_TB;

architecture Behavioral of motor_TB is
    component motor is
        generic(
            clk_div_log2: integer := din_length
        );
        Port (
            speed: in std_logic_vector (clk_div_log2-1 downto 0);
            direction: in std_logic;
            clk: in std_logic;
            clk_fast: in std_logic;
            PWM_out, CW, CCW: out std_logic
        );
    end component;

    signal t_speed: std_logic_vector (din_length-1 downto 0) := (others => '0');
    signal t_direction, t_clk, t_clk_fast, t_PWM_out, t_CW, t_CCW: std_logic := '0';
    signal t_cntr: integer range 0 to din_length-1 := 0;

begin
    inst_motor: motor
        port map(
            speed => t_speed,
            direction => t_direction,
            clk => t_clk,
            clk_fast => t_clk_fast,
            PWM_out => t_PWM_out,
            CW => t_CW,
            CCW => t_CCW
        );

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
            if(t_cntr < din_length-1) then
                t_cntr <= t_cntr + 1;
            else
                t_cntr <= 0;
                if(to_integer(unsigned(t_speed)) < 2**din_length-1) then
                    t_speed <= std_logic_vector(to_unsigned(to_integer(unsigned(t_speed)) + 1, din_length));
                else
                    t_speed <= (others => '0');
                    t_direction <= not t_direction;
                end if;
            end if;
        end if;
    end process;

end Behavioral;
