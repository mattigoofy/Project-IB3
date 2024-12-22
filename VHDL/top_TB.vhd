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

    constant clk_period : time := 10 ns;  -- 100 MHz 
    constant baud_rate_period : time := 104 us;  -- 9600 


    signal clk: std_logic := '0';
    signal UART_in: std_logic := '1';
    signal PWM_out: std_logic_vector(3 downto 0);
    signal CW: std_logic_vector(3 downto 0);
    signal CCW: std_logic_vector(3 downto 0);
    signal UART_out: std_logic;
    signal from_sensor: std_logic := '0';
    signal to_sensor: std_logic;

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
    uut: top
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

    clk_process : process
    begin
        while true loop
            clk <= '0';
            wait for clk_period / 2;
            clk <= '1';
            wait for clk_period / 2;
        end loop;
    end process;

    stim_process: process
    begin
        wait for 1 * baud_rate_period;

        for byte_index in 0 to bytes_to_send'length - 1 loop
            UART_in <= '0';  -- Start bit
            wait for baud_rate_period;

            for bit_index in 0 to 7 loop
                UART_in <= bytes_to_send(byte_index)(bit_index);
                wait for baud_rate_period;
            end loop;

            UART_in <= '1';  -- End bit
            wait for baud_rate_period;

            wait for 10 * baud_rate_period;
        end loop;

        wait for 2 ms;

        
        for i in 0 to 50 loop 
            from_sensor <= '1';
            wait for 12.5 us; 
            from_sensor <= '0';
            wait for 12.5 us;
        end loop;

        wait;
    end process;

end behavior;