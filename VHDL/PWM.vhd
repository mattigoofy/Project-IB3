library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use IEEE.numeric_std.ALL;

entity PWM is
    generic (
        size_din: integer := 5;
        clk_times_faster: integer := 32;
        clk_times_faster_log2: integer := 5
    );
    Port (
        din: std_logic_vector (size_din-1 downto 0);
        clk: in std_logic;
        clk_fast: in std_logic;
        dout: out std_logic
    );
end PWM;

architecture Behavioral of PWM is
    signal cntr: integer range 0 to clk_times_faster-1 := 1;
    signal prcnt: integer := 0;
    signal prcnt_vector, temp_prcnt: std_logic_vector (size_din+clk_times_faster_log2-1 downto 0) := (others => '0');
    signal out_buffer: std_logic_vector (clk_times_faster-1 downto 0) := (others => '0');
    signal leading_zeros: std_logic_vector (clk_times_faster_log2 downto 0) := (others => '0');
begin
    process(clk)
    begin
        if(rising_edge(clk)) then
            temp_prcnt <= std_logic_vector(resize(shift_left(unsigned(leading_zeros & din), clk_times_faster_log2), size_din+clk_times_faster_log2));
            prcnt_vector <= std_logic_vector(shift_right(unsigned(temp_prcnt), size_din));
            prcnt <= to_integer(unsigned(prcnt_vector));
            out_buffer <= (others => '0');
            out_buffer(prcnt downto 0) <= (others => '1');
        end if;
    end process;

    process(clk_fast)
    begin
        if(rising_edge(clk_fast)) then
            if(cntr < clk_times_faster-1) then
                cntr <= cntr + 1;
            else
                cntr <= 0;
            end if;
            dout <= out_buffer(cntr);
        end if;
    end process;

end Behavioral;
