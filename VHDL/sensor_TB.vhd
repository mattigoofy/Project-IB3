library IEEE;
use IEEE.std_logic_1164.ALL;
use IEEE.numeric_std.ALL;

entity sensor_TB is
end sensor_TB;

architecture behavior of sensor_TB is

    -- Component declaration for the sensor
    component sensor
        generic (
            freq_sens: integer := 40000;
            freq_clk: integer := 100000000;
            ammount_of_pulses: integer := 50
        );
        port (
            clk: in std_logic;
            start: in std_logic;
            sensor_out: in std_logic;
            sensor_in: out std_logic;
            dout: out integer range 0 to 65536;
            new_data: out std_logic
        );
    end component;

    -- Testbench signals
    signal clk: std_logic := '0';
    signal start: std_logic := '0';
    signal sensor_out: std_logic := '0';
    signal sensor_in: std_logic;
    signal dout: integer range 0 to 65536;
    signal new_data: std_logic;

    -- Clock period definition
    constant clk_period: time := 10 ns;  -- 100 MHz clock

begin

    -- Instantiate the sensor
    uut: sensor
        generic map (
            freq_sens => 40000,
            freq_clk => 100000000,
            ammount_of_pulses => 50
        )
        port map (
            clk => clk,
            start => start,
            sensor_out => sensor_out,
            sensor_in => sensor_in,
            dout => dout,
            new_data => new_data
        );

    -- Clock generation process
    clk_process: process
    begin
        while true loop
            clk <= '0';
            wait for clk_period / 2;
            clk <= '1';
            wait for clk_period / 2;
        end loop;
    end process;

    -- Stimulus process
    stimulus_process: process
    begin
        -- Start the sensor
        start <= '1';
        wait for clk_period;  -- Hold start high for 100 ns
        start <= '0';

        -- Simulate sensor output at 40 kHz for some time
        wait for 10 ms;  -- Wait for some time before starting the output

        -- Generate 40 kHz signal for sensor_out
        for i in 0 to 50 loop  -- Simulate 200 cycles (5 ms total)
            sensor_out <= '1';
            wait for 12.5 us;  -- High for 12.5 us (1/2 of 40 kHz period)
            sensor_out <= '0';
            wait for 12.5 us;  -- Low for 12.5 us
        end loop;

        -- Finish simulation
        wait;  -- Wait indefinitely
    end process;

end behavior;