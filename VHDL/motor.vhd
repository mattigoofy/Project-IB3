library IEEE;
use IEEE.STD_LOGIC_1164.ALL;

entity motor is
    generic(
        clk_div_log2: integer := 4
    );
    Port (
        speed: in std_logic_vector (clk_div_log2-1 downto 0);
        direction: in std_logic;
        clk: in std_logic;
        clk_fast: in std_logic;
        PWM_out, CW, CCW: out std_logic
    );
end motor;

architecture Behavioral of motor is
    component PWM is
        generic (
            size_din: integer := clk_div_log2;
            clk_times_faster: integer := 2**clk_div_log2;
            clk_times_faster_log2: integer := clk_div_log2
        );
        Port (
            din: std_logic_vector (size_din-1 downto 0);
            clk: in std_logic;
            clk_fast: in std_logic;
            dout: out std_logic
        );
    end component;

    signal prev_direction: std_logic := direction;
    signal cntr: integer range 0 to 3 := 0;

begin
    inst_pwm: PWM
        port map(
            din => speed,
            clk => clk,
            clk_fast => clk_fast,
            dout => PWM_out
        );


    CW <= direction;
    CCW <= not direction;

end Behavioral;
