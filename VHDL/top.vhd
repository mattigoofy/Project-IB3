library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use IEEE.NUMERIC_STD.ALL;


entity top is
    Port ( 
        clk: in std_logic;
        -- UART
        UART_in: in std_logic;
        PWM_out, CW, CCW: out std_logic_vector (3 downto 0);     -- LV, RV, LA, RA
        UART_out: out std_logic;

        from_sensor: in std_logic;
        to_sensor: out std_logic
    );
end top;

architecture Behavioral of top is  

    constant baud_rate: integer := 9600;
    constant clock_frequency: integer := 96000000;
    constant speed_length: integer := 5;
    constant direction_length: integer := 3;

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
            clk_motors_fast   : out    std_logic;
            clk_motors_slow   : out    std_logic;
            -- Status and control signals
            clk_in1           : in     std_logic
        );
    end component;

    -- UART
    component uart is
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
    end component;

    component sensor is
        generic (
            freq_sens: integer;
            freq_clk: integer;
            ammount_of_pulses: integer
        );
        port (
            clk: in std_logic;
            start: in std_logic;
            sensor_out: in std_logic;
            sensor_in: out std_logic;
            dout: out integer range 0 to 65536;
            new_data: out std_logic
        ) ;
    end component;

    type matrix is array (0 to 3) of std_logic_vector (speed_length-1 downto 0);
    signal s_speed: matrix;
    signal s_direction: std_logic_vector (3 downto 0);
    signal clk_main, clk_motors_slow_5, clk_motors_main: std_logic;
    signal speed: std_logic_vector (speed_length-1 downto 0);
    signal direction, prev_direction: std_logic_vector (direction_length-1 downto 0);

    signal uart_byte: std_logic_vector (7 downto 0) := (others => '1');
    signal new_byte: std_logic;
    signal sync_UART: std_logic;

    signal start_sensor: std_logic;
    signal sensor_data: integer range 0 to 65536;
    signal s_sensor_data: std_logic_vector(15 downto 0);
    signal sensor_new_data: std_logic;
    signal sync_from_sensor: std_logic;

begin
        
    inst_sync_UART: synchronizer
        generic map (
            length => 1
        )
        port map(
            din(0) => UART_in,
            clk => clk_main,
            dout(0) => sync_UART
        );

    inst_sync_sensor: synchronizer
        generic map (
            length => 1
        )
        port map(
            din(0) => from_sensor,
            clk => clk_main,
            dout(0) => sync_from_sensor
        );

    -- motors
    inst_motor_LV: motor
        port map (
            speed => s_speed(3),
            direction  => s_direction(3),
            clk => clk_motors_slow_5,
            clk_fast => clk_motors_main,
            PWM_out => PWM_out(3),
            CW => CW(3),
            CCW => CCW(3)
        );

        
    inst_motor_RV: motor
        port map (
            speed => s_speed(2),
            direction  => s_direction(2),
            clk => clk_motors_slow_5,
            clk_fast => clk_motors_main,
            PWM_out => PWM_out(2),
            CW => CW(2),
            CCW => CCW(2)
        );
        
    inst_motor_LA: motor
        port map (
            speed => s_speed(1),
            direction  => s_direction(1),
            clk => clk_motors_slow_5,
            clk_fast => clk_motors_main,
            PWM_out => PWM_out(1),
            CW => CW(1),
            CCW => CCW(1)
        );

    
    inst_motor_RA: motor
        port map (
            speed => s_speed(0),
            direction  => s_direction(0),
            clk => clk_motors_slow_5,
            clk_fast => clk_motors_main,
            PWM_out => PWM_out(0),
            CW => CW(0),
            CCW => CCW(0)
        );


    inst_MMCM : clk_wiz_0
        port map ( 
            clk_main => clk_main,
            clk_motors_slow => clk_motors_main,
            clk_motors_fast => clk_motors_slow_5,
            clk_in1 => clk
        );

    inst_uart: uart
        generic map (
            BAUD_RATE => baud_rate,
            CLOCK_FREQ => clock_frequency,
            din_size => 16
        )
        port map (
            clk => clk_main,
            UART_in => sync_UART,
            new_data => new_byte,
            dout => uart_byte,
            din => s_sensor_data,
            start_reading => sensor_new_data,
            UART_out => UART_out
        );

    inst_sensor: sensor
        generic map (
            freq_sens => 40000,
            freq_clk => clock_frequency,
            ammount_of_pulses => 50
        )
        port map (
            clk => clk_main,
            start => start_sensor,
            sensor_out => sync_from_sensor,
            sensor_in => to_sensor,
            dout => sensor_data,
            new_data => sensor_new_data
        );



    process(clk_main)
    begin
        if(rising_edge(clk_main)) then

            start_sensor <= '0';
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

                when "110" =>
                    start_sensor <= '1';
                    s_direction <= s_direction;
                    s_speed <= s_speed;
                    direction <= prev_direction;

                when others =>              -- default = idle
                    s_direction <= "1111";
                    s_speed <= (others => (others => '0'));

            end case;


            prev_direction <= direction;


            if new_byte = '1' then
                direction <= uart_byte(7 downto 5);
                speed <= uart_byte(4 downto 0);
            end if;
        end if;
    end process;

    s_sensor_data <= std_logic_vector(to_unsigned(sensor_data, 16));

end Behavioral;
