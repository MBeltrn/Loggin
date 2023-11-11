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
            InitializeControls(); 
            // Dejamos el foco en el Btn 
            this.ActiveControl = btnLogin;
        }

        private void InitializeControls()
        { 
            txtUsername = CreatePlaceholderTextBox("Email", new System.Drawing.Point(100, 50));

            txtPassword = CreatePlaceholderTextBox("Contraseña", new System.Drawing.Point(100, 80), true);

            btnLogin = new Button
            {
                Text = "Iniciar Sesión",
                Location = new System.Drawing.Point(100, 110),
                Size = new System.Drawing.Size(150, 30)
            };
            btnLogin.Click += BtnLogin_Click; 

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

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            // Calcula el hash de la contraseña
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
                // El usuario existe, verifica la contraseña y el estado
                string passwordQuery = "SELECT Contraseña FROM Usuarios WHERE NombreUsuario = @Username";
                using SqlCommand passwordCommand = new SqlCommand(passwordQuery, connection);
                passwordCommand.Parameters.AddWithValue("@Username", username);

                using SqlDataReader reader = passwordCommand.ExecuteReader();
                if (reader.Read())
                {
                    // Obtener la contraseña desde la columna "Contraseña"
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
                        if (VerifyPassword(hashedPassword, storedPassword))
                        {
                            // Iniciar sesión exitosamente
                            MessageBox.Show("Inicio de sesión exitoso.", "Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
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
            else
            {
                // El usuario no existe
                MessageBox.Show("El usuario no existe.", "Inicio de Sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            // Compara el hash de la contraseña proporcionada con el hash almacenado
            return inputPassword == storedPassword;
        }

        private static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            // Calcula el hash de la contraseña
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
