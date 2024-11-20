library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
--use IEEE.NUMERIC_STD.ALL;


entity top is
    generic (
        baud_rate: integer := 9600;
        clock_frequency: integer := 100000000;
        speed_length: integer := 5;
        direction_length: integer := 3
    );
    Port ( 
        clk: in std_logic;
        -- for testing
        -- speed: in std_logic_vector (speed_length-1 downto 0);
        -- direction: in std_logic_vector (direction_length-1 downto 0);
        -- UART
        UART_in: in std_logic;
        direction_temp: out std_logic_vector (2 downto 0);
        speed_temp: out std_logic_vector (4 downto 0);
        PWM_out, CW, CCW: out std_logic_vector (3 downto 0)     -- LV, RV, LA, RA
    );
end top;

architecture Behavioral of top is  

    component synchronizer
        generic(
            length: integer := 5
        );
        port(
            din: in std_logic_vector (length-1 downto 0);
            clk: in std_logic;
            dout: out std_logic_vector (length-1 downto 0)
        );
    end component;

    component motor
        generic(
            clk_div_log2: integer := 5
        );
        Port (
            speed: in std_logic_vector (clk_div_log2-1 downto 0);
            direction: in std_logic;
            clk: in std_logic;
            clk_fast: in std_logic;
            PWM_out, CW, CCW: out std_logic
        );
    end component;

    component clk_wiz_0
        port
        (-- Clock in ports
            -- Clock out ports
            clk_main          : out    std_logic;
            clk_slow_4          : out    std_logic;
            clk_slow_5          : out    std_logic;
            clk_slow_8          : out    std_logic;
            clk2_slow          : out    std_logic;
            clk2_fast          : out    std_logic;
            -- Status and control signals
            locked            : out    std_logic;
            clk_in1           : in     std_logic
        );
    end component;

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

    type matrix is array (0 to 3) of std_logic_vector (speed_length-1 downto 0);
    signal s_speed: matrix;
    signal s_direction: std_logic_vector (3 downto 0);
    signal clk_main, clk_slow_5, clk_100M: std_logic;
    signal speed: std_logic_vector (speed_length-1 downto 0);
    signal direction: std_logic_vector (direction_length-1 downto 0);

    signal uart_byte: std_logic_vector (7 downto 0) := (others => '1');
    signal new_byte: std_logic;
    signal sync_UART: std_logic;
begin
    -- synchronizers
    -- inst_sync_speed: synchronizer
    --     generic map (
    --         length => speed_length
    --     )
    --     port map(
    --         din => speed,
    --         clk => clk_slow_5,
    --         dout => sync_speed
    --     );
        
    -- inst_sync_direction: synchronizer
    --     generic map (
    --         length => direction_length
    --     )
    --     port map(
    --         din => direction,
    --         clk => clk_slow_5,
    --         dout => sync_direction
    --     );
        
    inst_sync_UART: synchronizer
        generic map (
            length => 1
        )
        port map(
            din(0) => UART_in,
            clk => clk_slow_5,
            dout(0) => sync_UART
        );

    -- motors
    inst_motor_LV: motor
        port map (
            speed => s_speed(3),
            direction  => s_direction(3),
            clk => clk_slow_5,
            clk_fast => clk_main,
            PWM_out => PWM_out(3),
            CW => CW(3),
            CCW => CCW(3)
        );

        
    inst_motor_RV: motor
        port map (
            speed => s_speed(2),
            direction  => s_direction(2),
            clk => clk_slow_5,
            clk_fast => clk_main,
            PWM_out => PWM_out(2),
            CW => CW(2),
            CCW => CCW(2)
        );
        
    inst_motor_LA: motor
        port map (
            speed => s_speed(1),
            direction  => s_direction(1),
            clk => clk_slow_5,
            clk_fast => clk_main,
            PWM_out => PWM_out(1),
            CW => CW(1),
            CCW => CCW(1)
        );

    
    inst_motor_RA: motor
        port map (
            speed => s_speed(0),
            direction  => s_direction(0),
            clk => clk_slow_5,
            clk_fast => clk_main,
            PWM_out => PWM_out(0),
            CW => CW(0),
            CCW => CCW(0)
        );


    inst_PLL : clk_wiz_0
        port map ( 
            -- Clock out ports  
            clk_main => clk_100M,
            clk_slow_4 => open,
            clk_slow_5 => open,
            clk_slow_8 => open,
            clk2_slow => clk_main,
            clk2_fast => clk_slow_5,
            -- Status and control signals                
            -- reset => '1',
            locked => open,
            -- Clock in ports
            clk_in1 => clk
        );

    inst_uart_bits: uart_bits
        generic map (
            BAUD_RATE => baud_rate,
            CLOCK_FREQ => clock_frequency
        )
        port map (
            clk => clk_100M,
            din => sync_UART,
            new_data => new_byte,
            dout => uart_byte
        );


    process(clk_100M)
    begin
        if rising_edge(clk_100M) then
            if new_byte = '1' then
                direction <= uart_byte(7 downto 5);
                speed <= uart_byte(4 downto 0);
            end if;
        end if;
    end process;


    process(clk_slow_5)
    begin
        if(rising_edge(clk_slow_5)) then
            case direction is
                when "000" =>               -- Forwards
                    s_direction <= "1100";
                    s_speed <= (others => speed);
                    
                when "001" =>               -- Backwards
                    s_direction <= "0011";
                    s_speed <= (others => speed);
                
                when "010" =>               -- Left
                    s_direction <= "0101";
                    s_speed <= (others => speed);
                    
                when "011" =>               -- Right
                    s_direction <= "1010";
                    s_speed <= (others => speed);
                    
                when "100" =>               -- Turn Left
                    s_direction <= "0110";
                    s_speed <= (others => speed);
                    
                when "101" =>               -- Turn Right
                    s_direction <= "1001";
                    s_speed <= (others => speed);

                when others =>              -- default = idle
                    s_direction <= "1111";
                    s_speed <= (others => (others => '0'));

            end case;
        end if;
    end process;

    direction_temp <= direction;
    speed_temp <= speed;

end Behavioral;
