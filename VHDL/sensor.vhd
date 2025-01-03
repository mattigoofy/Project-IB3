library IEEE;
use IEEE.std_logic_1164.ALL;
use IEEE.numeric_std.ALL;

entity sensor is
    generic (
        freq_sens: integer := 40000;
        freq_clk: integer := 100000000;
        ammount_of_pulses: integer := 50;
        max_length: integer := 10000
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
    constant max_ticks: integer := ( max_length * (freq_clk/1000) )/331;

    signal start_cntr: integer range 0 to wait_for := 0;
    signal time_cntr: integer range 0 to max_ticks := 0;
    signal prev_sensor_out: std_logic := '1';

    type fsm_states is (INIT, COUNTING);
    signal state_from: fsm_states := INIT;
    signal state_to: fsm_states := INIT;

begin

    to_sensor: process(clk)
    begin
        if rising_edge(clk) then
            case state_to is
                when INIT =>
                    if start = '1' then         -- starting the output
                        start_cntr <= 0;
                        sensor_in <= '1';
                        state_to <= COUNTING;
                    else
                        sensor_in <= '0';
                    end if;
                
                when COUNTING =>
                    if start_cntr < wait_for then
                        start_cntr <= start_cntr + 1;
                    else 
                        sensor_in <= '0';
                        state_to <= INIT;
                    end if;
            end case;
        end if;
    end process;

    from_sensor: process(clk)
    begin
        if rising_edge(clk) then
            case state_from is
                when INIT =>
                    new_data <= '0';
                    if start = '1' then
                        state_from <= COUNTING;
                    end if;
                    time_cntr <= 0;

                when COUNTING =>
                    if sensor_out > prev_sensor_out then      -- rising edge
                        state_from <= INIT;
                        dout <= 331*time_cntr/(freq_clk/1000);
                        new_data <= '1';
                    else
                        if time_cntr < max_ticks then
                            time_cntr <= time_cntr + 1;
                        else    
                            state_from <= INIT;
                            dout <= 65535;
                            new_data <= '1';
                        end if;
                        
                    end if;
                    prev_sensor_out <= sensor_out;
            end case;
        end if;
    end process;
end arch ; -- arch