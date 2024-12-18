library IEEE;
use IEEE.std_logic_1164.ALL;
use IEEE.numeric_std.ALL;

entity uart_send is
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
end uart_send;

architecture arch of uart_send is

    component clk_div is
        generic ( n : natural);
        Port ( clk_in : in STD_LOGIC;
               clk_out : out STD_LOGIC);
    end component;

    signal dbuffer: std_logic_vector(din_size - 1 + 4 downto 0);
    
    type fsm_states is (READY, SENDING);
    signal state: fsm_states := READY;

    signal index: integer range 0 to din_size - 1 + 4 := 0;
    signal clk_uart: std_logic;
    
begin


    inst_div: clk_div
        generic map (n => CLOCK_FREQ/BAUD_RATE)
        port map (
            clk_in => clk, 
            clk_out => clk_uart
        );


    process(clk)
    begin
        if rising_edge(clk) then
            case state is
                when READY =>
                    if start = '1' then
                        dbuffer <= '1' & din(15 downto 8) & '0' & '1' & din(7 downto 0) & '0';
                        -- dout <= '0';
                        state <= SENDING;
                    else
                        -- dout <= '1';
                        dbuffer <= dbuffer;
                    end if;

                when SENDING =>
                    if index < din_size - 1 + 4 then
                        -- index <= index + 1;
                    else 
                        -- index <= 0;
                        state <= READY;
                        -- dout <= '1';
                    end if;
                    -- dout <= dbuffer(index);
                
                when others =>
                    state <= state;

            end case;
        end if;
    end process;

    process(clk_uart)
    begin
        if rising_edge(clk_uart) then
            case state is
                when READY =>
                    index <= 0;
                    dout <= '1';

                    -- if prev_start = '1' then
                    --     dout <= '0';
                    -- end if;

                    -- prev_start <= start;
                when SENDING =>
                    dout <= dbuffer(index);
                        if index < din_size - 1 + 4 then
                            index <= index + 1;
                        else 
                            index <= 0;
                            -- state <= READY;
                            dout <= '1';
                        end if;
                when others =>
                    index <= index;
            end case;
        end if;
    end process;


end arch ; -- arch