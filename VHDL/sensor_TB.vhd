library IEEE;
use IEEE.std_logic_1164.ALL;
use IEEE.numeric_std.ALL;

entity sensor_TB is
end sensor_TB;

architecture behavior of sensor_TB is

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

    signal clk: std_logic := '0';
    signal start: std_logic := '0';
    signal sensor_out: std_logic := '0';
    signal sensor_in: std_logic := '0';
    signal dout: integer range 0 to 65536;
    signal new_data: std_logic;

    constant clk_period: time := 10 ns;  -- 100 MHz clock

begin
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

    clk_process: process
    begin
        while true loop
            clk <= '0';
            wait for clk_period / 2;
            clk <= '1';
            wait for clk_period / 2;
        end loop;
    end process;

    stimulus_process: process
    begin
        start <= '1';
        wait for clk_period; 
        start <= '0';


        wait for 23.24 ms; 

        -- 40 kHz signal
        for i in 0 to 50 loop
            sensor_out <= '1';
            wait for 12.5 us;
            sensor_out <= '0';
            wait for 12.5 us;
        end loop;
        
        start <= '1';
        wait for clk_period;
        start <= '0';

        wait for 100 ms;

        -- 40 kHz signal
        for i in 0 to 50 loop
            sensor_out <= '1';
            wait for 12.5 us;
            sensor_out <= '0';
            wait for 12.5 us;
        end loop;
    end process;

end behavior;