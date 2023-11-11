using System;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Loggin
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;

        public LoginForm()
        {
            InitializeControls(); // Llama a un método para crear los controles.

            // Establecer el enfoque en el botón de inicio de sesión
            this.ActiveControl = btnLogin;
        }

        private void InitializeControls()
        {
            // Crea un cuadro de texto para el nombre de usuario con texto de marcador de posición
            txtUsername = CreatePlaceholderTextBox("Email", new Point(100, 50));

            // Crea un cuadro de texto para la contraseña con texto de marcador de posición
            txtPassword = CreatePlaceholderTextBox("Contraseña", new Point(100, 80), true);

            // Crea un botón para iniciar sesión
            btnLogin = new Button();
            btnLogin.Text = "Iniciar Sesión";
            btnLogin.Location = new Point(100, 110);
            btnLogin.Size = new Size(150, 30);
            btnLogin.Click += btnLogin_Click; // Asocia un evento al botón

            // Agrega controles al formulario
            this.Controls.Add(txtUsername);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private TextBox CreatePlaceholderTextBox(string placeholderText, Point location, bool isPassword = false)
        {
            TextBox textBox = new TextBox();
            textBox.Location = location;
            textBox.Size = new Size(150, 20);

            // Evento Enter: cuando el cuadro de texto obtiene el foco, si contiene el texto del marcador de posición, lo elimina.
            textBox.Enter += (sender, e) =>
            {
                if (textBox.Text == placeholderText)
                {
                    textBox.Text = "";
                    if (isPassword)
                    {
                        textBox.PasswordChar = '*'; // Oculta la contraseña
                    }
                }
            };

            // Evento Leave: cuando el cuadro de texto pierde el foco y está vacío, vuelve a mostrar el texto del marcador de posición.
            textBox.Leave += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholderText;
                    if (isPassword)
                    {
                        textBox.PasswordChar = '\0'; // Muestra el texto normal
                    }
                }
            };

            // Establece el texto de marcador de posición inicial.
            textBox.Text = placeholderText;

            if (isPassword)
            {
                textBox.PasswordChar = '\0'; // Muestra el texto normal
            }

            return textBox;
        }


        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            string connectionString = "DESKTOP-K9F4KBH"; // Reemplaza con la cadena de conexión correcta
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Verificar si el usuario existe
                string query = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @Username";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                        // El usuario existe, verifica la contraseña y el estado
                        string passwordQuery = "SELECT Contraseña, Estado FROM Usuarios WHERE NombreUsuario = @Username";
                        using (SqlCommand passwordCommand = new SqlCommand(passwordQuery, connection))
                        {
                            passwordCommand.Parameters.AddWithValue("@Username", username);

                            using (SqlDataReader reader = passwordCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string storedPassword = reader["Contraseña"].ToString();
                                    int userState = Convert.ToInt32(reader["Estado"]);

                                    if (userState == 0)
                                    {
                                        // El usuario está bloqueado
                                        MessageBox.Show("Debe contactarse con el Administrador. El usuario está bloqueado.", "Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                    else
                                    {
                                        // Verificar la contraseña
                                        if (VerifyPassword(password, storedPassword))
                                        {
                                            // Iniciar sesión exitosamente
                                            MessageBox.Show("Inicio de sesión exitoso.", "Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            // Aquí puedes abrir la ventana principal de la aplicación
                                        }
                                        else
                                        {
                                            // Contraseña incorrecta
                                            MessageBox.Show("Contraseña incorrecta.", "Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                                else
                                {
                                    // Usuario no encontrado
                                    MessageBox.Show("El usuario no existe.", "Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    else
                    {
                        // El usuario no existe
                        MessageBox.Show("El usuario no existe.", "Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


    }
}