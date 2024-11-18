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

    -- ILA
    -- COMPONENT ila_0

    -- PORT (
    --     clk : IN STD_LOGIC;

    --     probe0 : IN STD_LOGIC_VECTOR(0 DOWNTO 0); 
    --     probe1 : IN STD_LOGIC_VECTOR(7 DOWNTO 0); 
    --     probe2 : IN STD_LOGIC_VECTOR(0 DOWNTO 0);
    --     probe3 : IN STD_LOGIC_VECTOR(0 DOWNTO 0);
    --     probe4 : IN STD_LOGIC_VECTOR(3 DOWNTO 0)
    -- );
    -- END COMPONENT  ;

    -- signal s_new_data: std_logic;
    -- signal s_dout: std_logic_vector (7 downto 0);
    -- signal s_data_cntr: std_logic_vector (3 downto 0);

begin

    -- inst_ila : ila_0
    --     PORT MAP (
    --         clk => clk,

    --         probe0(0) => din, 
    --         probe1 => s_dout, 
    --         probe2(0) => s_new_data,
    --         probe3(0) => din,
    --         probe4 => s_data_cntr
    --     );

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



    -- process(clk)
    -- begin
    --     if rising_edge(clk) then
    --         case state is
    --             when READY =>
    --                 new_data <= '0';
    --                 s_new_data <= '0';
    --                 if(dbuffer(7) = '1' and din = '0') then  -- Detect start bit
    --                     state <= READING;
    --                     data_cntr <= 0;
    --                     clk_cntr <= 0;  -- Reset counter at start bit
    --                 end if;
    --                 dbuffer <= din & dbuffer(7 downto 1);

    --             when READING =>
    --                 if (clk_cntr = CLOCK_FREQ / BAUD_RATE/2) then
    --                     -- clk_cntr <= 0;  -- Reset counter for next bit sampling
    --                     data_cntr <= data_cntr + 1;
                        
    --                     if data_cntr = 8 then  -- End of data bits
    --                         dout <= dbuffer;   -- Load received data into output
    --                         new_data <= '1';
    --                         s_new_data <= '1';
    --                         state <= READY;  -- Return to READY after stop bit
    --                     else
    --                         dbuffer <= din & dbuffer(7 downto 1); -- Shift data
    --                     end if;
    --                 else
    --                     clk_cntr <= clk_cntr + 1;
    --                 end if;
    --         end case;
    --     end if;
    -- end process;


    process(clk)
    begin
        if rising_edge(clk) then            
            case state is
                when READY =>
                    new_data <= '0';
                    -- s_new_data <= '0';
                    if(dbuffer(7) = '1' and din = '0') then    -- start bit
                        state <= READING;
                        data_cntr <= 0;
                    end if;

                when READING =>
                    if(data_cntr = 9 and din = '1') then    -- end bit
                        data_cntr <= 0;
                        new_data <= '1';
                        -- s_new_data <= '1';
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
    -- dout  <= s_dout;
    -- s_data_cntr <= std_logic_vector(to_unsigned(data_cntr, 4));

end arch ; -- arch