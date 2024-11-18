library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use IEEE.STD_LOGIC_ARITH.ALL;
use IEEE.STD_LOGIC_UNSIGNED.ALL;

entity top_TB is
    generic(
        clk_period : time := 10 ns;  -- 100 MHz clock
        baud_rate_period : time := 104us  -- 9600 bps
    );
end top_TB;

architecture behavior of top_TB is
    -- Component Declaration for the Unit Under Test (UUT)
    component top
        generic (
            baud_rate: integer := 9600;
            clock_frequency: integer := 100000000;
            speed_length: integer := 5;
            direction_length: integer := 3
        );
        Port ( 
            clk: in std_logic;
            UART_in: in std_logic;
            PWM_out, CW, CCW: out std_logic_vector (3 downto 0)  -- LV, RV, LA, RA
        );
    end component;

    -- Signals for connecting to the UUT
    signal clk: std_logic := '0';
    signal UART_in: std_logic := '1';  -- idle state for UART
    signal PWM_out: std_logic_vector(3 downto 0);
    signal CW: std_logic_vector(3 downto 0);
    signal CCW: std_logic_vector(3 downto 0);

    -- Array of bytes to send
    type byte_array is array (0 to 5) of std_logic_vector(7 downto 0);
    signal bytes_to_send : byte_array := (
        "00010000",  -- Byte 0
        "10010010",  -- Byte 1
        "01011111",  -- Byte 2
        "00100011",  -- Byte 3
        "00000100",  -- Byte 4
        "10101010"   -- Byte 5
    );  
    signal byte_index: integer := 0;

begin
    -- Instantiate the Unit Under Test (UUT)
    uut: top
        generic map (
            baud_rate => 9600,
            clock_frequency => 100000000,
            speed_length => 5,
            direction_length => 3
        )
        port map (
            clk => clk,
            UART_in => UART_in,
            PWM_out => PWM_out,
            CW => CW,
            CCW => CCW
        );

    -- Clock generation process
    clk_process : process
    begin
        clk <= '0';
        wait for clk_period / 2;
        clk <= '1';
        wait for clk_period / 2;
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

        -- Finish simulation
        assert false report "End of simulation" severity note;
        wait;
    end process;

end behavior;