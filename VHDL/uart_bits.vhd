library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use IEEE.numeric_std.ALL;

entity uart_bits is
    generic(
        BAUD_RATE : integer := 9600;
        CLOCK_FREQ : integer := 100000000
    );
    port (
        clk: in std_logic;
        din: in std_logic;
        new_data: out std_logic;
        dout: out std_logic_vector (7 downto 0)
    ) ;
end uart_bits;

architecture arch of uart_bits is
    type fsm_states is (READY, READING);
    signal state: fsm_states := READY;

    signal clk_cntr: integer range 0 to CLOCK_FREQ/BAUD_RATE := 0;
    signal data_cntr: integer range 0 to 9 := 0;
    signal dbuffer: std_logic_vector (7 downto 0) := (others => '1');
    signal prev_din : std_logic := '1';

begin
    cntr_inc: process(clk)
    begin
        if(rising_edge(clk)) then
            if(din /= prev_din) then
                clk_cntr <= 0;
                prev_din <= din;
            else
                if(clk_cntr < CLOCK_FREQ/BAUD_RATE) then
                    clk_cntr <= clk_cntr + 1;
                else 
                clk_cntr <= 0;
                end if;
            end if;
        end if;
    end process;


    process(clk)
    begin
        if rising_edge(clk) then            
            case state is
                when READY =>
                    new_data <= '0';
                    if(dbuffer(7) = '1' and din = '0') then    -- start bit
                        state <= READING;
                        data_cntr <= 0;
                    end if;

                when READING =>
                    if(data_cntr = 9 and din = '1') then    -- end bit
                        data_cntr <= 0;
                        new_data <= '1';
                        dout <= dbuffer;
                        state <= READY;
                    end if;
            end case;

            if (clk_cntr = CLOCK_FREQ/BAUD_RATE/2) then
                dbuffer <= din & dbuffer(7 downto 1);

                if(data_cntr < 9) then
                    data_cntr <= data_cntr + 1;
                end if;
            end if;
        end if;
    end process;

end arch ; -- arch