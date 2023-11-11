using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Loggin
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;

        public LoginForm()
        {
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            InitializeControls(); // Llama a un m�todo para crear los controles.

            // Establecer el enfoque en el bot�n de inicio de sesi�n
            this.ActiveControl = btnLogin;
        }

        private void InitializeControls()
        {
            // Crea un cuadro de texto para el nombre de usuario con texto de marcador de posici�n
            txtUsername = CreatePlaceholderTextBox("Email", new System.Drawing.Point(100, 50));

            // Crea un cuadro de texto para la contrase�a con texto de marcador de posici�n
            txtPassword = CreatePlaceholderTextBox("Contrase�a", new System.Drawing.Point(100, 80), true);

            // Crea un bot�n para iniciar sesi�n
            btnLogin = new Button
            {
                Text = "Iniciar Sesi�n",
                Location = new System.Drawing.Point(100, 110),
                Size = new System.Drawing.Size(150, 30)
            };
            btnLogin.Click += BtnLogin_Click; // Asocia un evento al bot�n

            // Agrega controles al formulario
            this.Controls.Add(txtUsername);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private static TextBox CreatePlaceholderTextBox(string placeholderText, System.Drawing.Point location, bool isPassword = false)
        {
            TextBox textBox = new()
            {
                Location = location,
                Size = new System.Drawing.Size(150, 20)
            };

            // Evento Enter: cuando el cuadro de texto obtiene el foco, si contiene el texto del marcador de posici�n, lo elimina.
            textBox.Enter += (sender, e) =>
            {
                if (textBox.Text == placeholderText)
                {
                    textBox.Text = "";
                    if (isPassword)
                    {
                        textBox.PasswordChar = '*'; // Oculta la contrase�a
                    }
                }
            };

            // Evento Leave: cuando el cuadro de texto pierde el foco y est� vac�o, vuelve a mostrar el texto del marcador de posici�n.
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

            // Establece el texto de marcador de posici�n inicial.
            textBox.Text = placeholderText;

            if (isPassword)
            {
                textBox.PasswordChar = '\0'; // Muestra el texto normal
            }

            return textBox;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            // Calcula el hash de la contrase�a
            string hashedPassword = HashPassword(password);

            string connectionString = @"Data Source=DESKTOP-K9F4KBH;Initial Catalog=Loggin;Integrated Security=True;TrustServerCertificate=True";
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            // Verificar si el usuario existe
            string query = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @Username";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            int count = (int)command.ExecuteScalar();

            if (count > 0)
            {
                // El usuario existe, verifica la contrase�a y el estado
                string passwordQuery = "SELECT Contrase�a FROM Usuarios WHERE NombreUsuario = @Username";
                using SqlCommand passwordCommand = new SqlCommand(passwordQuery, connection);
                passwordCommand.Parameters.AddWithValue("@Username", username);

                using SqlDataReader reader = passwordCommand.ExecuteReader();
                if (reader.Read())
                {
                    // Obtener la contrase�a desde la columna "Contrase�a"
                    string storedPassword = reader["Contrase�a"].ToString();
                    int userState = Convert.ToInt32(reader["Estado"]);

                    if (userState == 0)
                    {
                        // El usuario est� bloqueado
                        MessageBox.Show("Debe contactarse con el Administrador. El usuario est� bloqueado.", "Inicio de Sesi�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        // Verificar la contrase�a
                        if (VerifyPassword(hashedPassword, storedPassword))
                        {
                            // Iniciar sesi�n exitosamente
                            MessageBox.Show("Inicio de sesi�n exitoso.", "Inicio de Sesi�n", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Aqu� puedes abrir la ventana principal de la aplicaci�n
                        }
                        else
                        {
                            // Contrase�a incorrecta
                            MessageBox.Show("Contrase�a incorrecta.", "Inicio de Sesi�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    // Usuario no encontrado
                    MessageBox.Show("El usuario no existe.", "Inicio de Sesi�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // El usuario no existe
                MessageBox.Show("El usuario no existe.", "Inicio de Sesi�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            // Compara el hash de la contrase�a proporcionada con el hash almacenado
            return inputPassword == storedPassword;
        }

        private static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            // Calcula el hash de la contrase�a
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Convierte los bytes a una cadena hexadecimal
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashedBytes.Length; i++)
            {
                builder.Append(hashedBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
