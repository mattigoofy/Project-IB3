library IEEE;
use IEEE.std_logic_1164.ALL;

entity sensor is
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
    ) ;
end sensor;

architecture arch of sensor is
    constant wait_for: integer := ammount_of_pulses * freq_clk / freq_sens;

    signal start_cntr: integer range 0 to wait_for := 0;
    signal time_cntr: integer range 0 to 65536 := 0;
    signal prev_sensor_out: std_logic := '1';

    type fsm_states is (INIT, COUNTING);
    signal state: fsm_states := INIT;
begin

    process(clk)
    begin
        if rising_edge(clk) then
            if start = '1' then         -- starting the output
                start_cntr <= 0;
                sensor_in <= '1';
            end if;

            if start_cntr < wait_for then
                start_cntr <= start_cntr + 1;
            else 
                sensor_in <= '0';
            end if;
        end if;
    end process;

    process(clk)
    begin
        if rising_edge(clk) then
            case state is
                when INIT =>
                    new_data <= '0';
                    if start = '1' then
                        state <= COUNTING;
                        time_cntr <= 0;
                    end if;

                when COUNTING =>
                    if sensor_out > prev_sensor_out then      -- rising edge
                        state <= INIT;
                        dout <= 331*time_cntr/(freq_clk/1000);
                        new_data <= '1';
                    else
                        time_cntr <= time_cntr + 1;
                    end if;
                    prev_sensor_out <= sensor_out;
            end case;
        end if;
    end process;
end arch ; -- arch