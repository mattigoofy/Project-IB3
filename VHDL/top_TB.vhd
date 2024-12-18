library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use IEEE.STD_LOGIC_ARITH.ALL;
use IEEE.STD_LOGIC_UNSIGNED.ALL;

entity tb_top is
end tb_top;

architecture behavior of tb_top is
    -- Component Declaration for the Unit Under Test (UUT)
    component top
        -- generic (
        --     baud_rate: integer := 9600;
        --     clock_frequency: integer := 100000000;
        --     speed_length: integer := 5;
        --     direction_length: integer := 3
        -- );
        Port ( 
            clk: in std_logic;
            UART_in: in std_logic;
            PWM_out, CW, CCW: out std_logic_vector (3 downto 0);  -- LV, RV, LA, RA
            UART_out: out std_logic;
            from_sensor: in std_logic;
            to_sensor: out std_logic
        );
    end component;

    -- Constants
    constant clk_period : time := 10 ns;  -- 100 MHz clock
    constant baud_rate_period : time := 104 us;  -- 9600 bps

    -- Signals for connecting to the UUT
    signal clk: std_logic := '0';
    signal UART_in: std_logic := '1';  -- idle state for UART
    signal PWM_out: std_logic_vector(3 downto 0);
    signal CW: std_logic_vector(3 downto 0);
    signal CCW: std_logic_vector(3 downto 0);
    signal UART_out: std_logic;
    signal from_sensor: std_logic := '0';
    signal to_sensor: std_logic;

    -- Array of bytes to send
    type byte_array is array (0 to 1) of std_logic_vector(7 downto 0);
    signal bytes_to_send : byte_array := (
        "00010000",  -- Byte 0
        -- "10010010",  -- Byte 1
        -- "01011111",  -- Byte 2
        -- "00100011",  -- Byte 3
        -- "00000100",  -- Byte 4
        "11001010"   -- Byte 5
    );  
    signal byte_index: integer := 0;

begin
    -- Instantiate the Unit Under Test (UUT)
    uut: top
        -- generic map (
        --     baud_rate => 9600,
        --     clock_frequency => 100000000,
        --     speed_length => 5,
        --     direction_length => 3
        -- )
        port map (
            clk => clk,
            UART_in => UART_in,
            PWM_out => PWM_out,
            CW => CW,
            CCW => CCW,
            UART_out => UART_out,
            from_sensor => from_sensor,
            to_sensor => to_sensor
        );

    -- Clock generation process
    clk_process : process
    begin
        while true loop
            clk <= '0';
            wait for clk_period / 2;
            clk <= '1';
            wait for clk_period / 2;
        end loop;
    end process;

    -- Stimulus process
    stim_process: process
    begin
        -- Wait for the initial state
        wait for 1 * baud_rate_period;

        -- Loop through each byte in the array
        for byte_index in 0 to bytes_to_send'length - 1 loop
            -- Simulate sending a start bit (0)
            UART_in <= '0';  -- Start bit
            wait for baud_rate_period;

            -- Send each bit of the byte
            for bit_index in 0 to 7 loop
                UART_in <= bytes_to_send(byte_index)(bit_index);  -- Send each bit
                wait for baud_rate_period;
            end loop;

            -- Simulate sending an end bit (1)
            UART_in <= '1';  -- End bit
            wait for baud_rate_period;

            -- Wait for a while to observe the output
            wait for 10 * baud_rate_period;
        end loop;

        wait for 2 ms;

        
        for i in 0 to 50 loop  -- Simulate 200 cycles (5 ms total)
            from_sensor <= '1';
            wait for 12.5 us;  -- High for 12.5 us (1/2 of 40 kHz period)
            from_sensor <= '0';
            wait for 12.5 us;  -- Low for 12.5 us
        end loop;

        -- Finish simulation
        -- assert false report "End of simulation" severity note;
        wait;
    end process;

end behavior;