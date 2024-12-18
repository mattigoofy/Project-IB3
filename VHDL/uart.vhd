library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use IEEE.NUMERIC_STD.ALL;

entity uart is
    generic(
        BAUD_RATE: integer;
        CLOCK_FREQ : integer;
        din_size: integer
    );
    port (
        clk: in std_logic;

        din: in std_logic_vector(din_size-1 downto 0);
        dout: out std_logic_vector (7 downto 0);

        start_reading: in std_logic;
        new_data: out std_logic;

        UART_in: in std_logic;
        UART_out: out std_logic
    );
end uart;

architecture arch of uart is

    -- UART
    component uart_bits is
        generic(
            BAUD_RATE : integer;
            CLOCK_FREQ : integer
        );
        port (
            clk: in std_logic;
            din: in std_logic;
            new_data: out std_logic;
            dout: out std_logic_vector (7 downto 0)
        );
    end component;

    component uart_send is
        generic (
            BAUD_RATE: integer;
            CLOCK_FREQ : integer;
            din_size: integer
        );
        port (
            clk: in std_logic;
            din: in std_logic_vector(din_size-1 downto 0);
            start: in std_logic;
            dout: out std_logic
        );
    end component;

begin
    
    inst_uart_bits: uart_bits
        generic map (
            BAUD_RATE => BAUD_RATE,
            CLOCK_FREQ => CLOCK_FREQ
        )
        port map (
            clk => clk,
            din => UART_in,
            new_data => new_data,
            dout => dout
        );

    inst_uart_send: uart_send
        generic map (
            BAUD_RATE => BAUD_RATE,
            CLOCK_FREQ => CLOCK_FREQ,
            din_size => din_size
        )
        port map (
            clk => clk,
            din => din,
            start => start_reading,
            dout => UART_out
        );

end arch ; -- arch