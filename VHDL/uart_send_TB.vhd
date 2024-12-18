library IEEE;
use IEEE.std_logic_1164.ALL;
use IEEE.numeric_std.ALL;

entity uart_send_TB is
end uart_send_TB;

architecture behavior of uart_send_TB is

    -- Component Declaration for the Unit Under Test (UUT)
    component uart_send
        generic (
            BAUD_RATE: integer := 9600;
            CLOCK_FREQ : integer := 96000000;
            din_size: integer := 16
        );
        port (
            clk: in std_logic;
            din: in std_logic_vector(din_size-1 downto 0);
            start: in std_logic;
            dout: out std_logic
        );
    end component;

    -- Signals for the testbench
    signal clk: std_logic := '0';
    signal clk_fast: std_logic := '0';
    signal din: std_logic_vector(15 downto 0) := (others => '0');
    signal start: std_logic := '0';
    signal dout: std_logic;

    -- Clock period definitions
    constant clk_period: time := 104.1666667 us; -- 50 MHz
    constant clk_fast_period: time := 10.41666667 ns; -- 100 MHz

begin

    -- Instantiate the Unit Under Test (UUT)
    uut: uart_send
        generic map (
            BAUD_RATE => 9600,
            CLOCK_FREQ => 96000000,
            din_size => 16
        )
        port map (
            clk => clk_fast,
            din => din,
            start => start,
            dout => dout
        );

    -- Fast clock process
    clk_fast_process: process
    begin
        while true loop
            clk_fast <= '0';
            wait for clk_fast_period / 2;
            clk_fast <= '1';
            wait for clk_fast_period / 2;
        end loop;
    end process;

    -- Stimulus process
    stim_process: process
    begin
        -- Wait for global reset
        wait for clk_period * 2;

        -- Test case 1: Send a value
        din <= "1010101010101010"; -- Example data
        start <= '1'; -- Start sending
        wait for clk_fast_period; -- Wait for a clock cycle
        start <= '0'; -- Stop sending

        -- Wait for some time to observe the output
        wait for clk_period * 20;

        -- Test case 2: Send another value
        din <= "1100110011001100"; -- Another example data
        start <= '1'; -- Start sending
        wait for clk_fast_period; -- Wait for a clock cycle
        start <= '0'; -- Stop sending

        -- Wait for some time to observe the output
        wait for clk_period * 20;

        -- Finish simulation
        wait;
    end process;

end behavior;